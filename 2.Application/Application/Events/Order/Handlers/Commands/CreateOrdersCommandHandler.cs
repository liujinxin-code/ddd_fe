using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Common.Models.Order;
using Application.Events.Order.Contracts.Commands;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Shared.Exceptions;
using Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Events.Order.Handlers.Commands
{
    /// <summary>
    /// 批量下单处理器。
    /// 流程：加载下单用户（追踪态）→ 批量加载配置 → 逐项校验（可用性/数量约束/链接）→
    /// 按当前用户定价计算单价与金额、计算代理利润 → 余额校验与扣减 → 批量落订单 → 写扣款流水 → 一次性提交。
    /// 整段放入 unitOfWork.ExecuteWithRetryAsync 的乐观并发重试，余额等核心字段不丢更新；
    /// 任一明细失败即整体回滚（原子），保证余额与订单一致。
    /// </summary>
    public class CreateOrdersCommandHandler(
        IConfigRepository configRepository,
        ITkUserRepository tkUserRepository,
        IOrderRepository orderRepository,
        IConsumeLogRepository consumeLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
        : IRequestHandler<CreateOrdersCommand, ApiResult<CreateOrderResult>>
    {
        public async Task<ApiResult<CreateOrderResult>> Handle(CreateOrdersCommand request, CancellationToken ct)
        {
            try
            {
                return await unitOfWork.ExecuteWithRetryAsync(async () =>
                {
                    var user = await tkUserRepository.GetByIdAsync(currentUser.Userid, ct);
                    if (user is null) throw new BusinessException("用户不存在");

                    // 加载本批涉及的全部配置（一次查询，避免 N+1）
                    var configIds = request.Items.Select(i => i.ConfigId).Distinct().ToList();
                    var configs = await configRepository.GetByIdsAsync(configIds, ct);
                    var configMap = configs.ToDictionary(c => c.ConfigId);

                    // 装配当前用户定价上下文（一次查询，供全部明细复用）
                    var priceCtx = await BuildPriceContextAsync(user, configIds, ct);

                    var orders = new List<TkOrder>(request.Items.Count);
                    decimal totalAmount = 0m;

                    foreach (var item in request.Items)
                    {
                        if (!configMap.TryGetValue(item.ConfigId, out var config))
                            throw new BusinessException($"业务配置不存在：ConfigId={item.ConfigId}");

                        // 业务须可用（状态非“未启用”，即允许 全部启用 与 仅限API）
                        config.RequiredEnabled();

                        bool isAccount = config.JsonTemplate == JsonTemplate.PurchaseAccount;
                        bool isComment = config.JsonTemplate == JsonTemplate.Comment;

                        // 链接要求：增量业务（涨粉/评论）必填；账户业务（买号）选填
                        string link = (item.OrderLink ?? string.Empty).Trim();
                        if (!isAccount && string.IsNullOrEmpty(link))
                            throw new BusinessException("增量业务必须填写下单链接");
                        if (link.Length > 500)
                            throw new BusinessException("下单链接不能超过 500 字符");

                        // 评论业务：一条评论 = 一个数量，订单数量恒等于评论条数
                        var comments = (item.Comments ?? new List<string>())
                            .Select(c => (c ?? string.Empty).Trim())
                            .Where(c => c.Length > 0)
                            .ToList();

                        int quantity = item.Quantity;
                        if (isComment)
                        {
                            if (comments.Count == 0)
                                throw new BusinessException("评论业务必须提交评论内容");
                            if (comments.Any(c => c.Length > 500))
                                throw new BusinessException("单条评论内容不能超过 500 字符");

                            // 数量由评论条数唯一决定，调用方传的 Quantity 一律忽略
                            quantity = comments.Count;
                        }

                        if (quantity <= 0)
                            throw new BusinessException("订单数量必须大于 0");

                        // 数量约束（min/max/order_unit）
                        config.RequiredQuantity(quantity);

                        // 当前用户最终单价与订单金额（金额由服务端计算，调用方无法篡改）
                        decimal unitPrice = ConfigPriceCalculator.CalculateUnitPrice(config, priceCtx);
                        decimal amount = Utils.RoundToSixDecimals(unitPrice * quantity);

                        // 代理利润（仅当下单用户有上级代理时计算差价）
                        decimal agentSingleAdd = 0m;
                        decimal agentOrderAmount = 0m;
                        long agentUserId = 0;
                        if (user.AgentUserid > 0)
                        {
                            agentUserId = user.AgentUserid;
                            decimal agentCost = priceCtx.AgentCustom.TryGetValue(config.ConfigId, out var ac) ? ac : config.ConfigPrice;
                            decimal profitPerUnit = unitPrice - agentCost;
                            if (profitPerUnit < 0) profitPerUnit = 0;
                            agentSingleAdd = Utils.RoundToSixDecimals(profitPerUnit);
                            agentOrderAmount = Utils.RoundToSixDecimals(profitPerUnit * quantity);
                        }

                        var orderNo = Utils.GenerateSerialNo(serialNoPre: "O");
                        var order = new TkOrder(
                            orderNo,
                            config.ConfigId,
                            user.Userid,
                            link,
                            amount,
                            quantity,
                            config.ChannelId,
                            config.ChannelServerId,
                            agentUserId,
                            agentSingleAdd,
                            agentOrderAmount);

                        // 评论内容随订单一起落 tk_comment（order_id 由 EF 在同一次 SaveChanges 回填）
                        if (isComment)
                        {
                            foreach (var content in comments)
                            {
                                order.AddCommentFunc(content, user.Userid);
                            }
                        }

                        orders.Add(order);

                        totalAmount += amount;
                    }

                    totalAmount = Utils.RoundToSixDecimals(totalAmount);

                    // 余额校验 + 扣减（原子，置于重试内，并发时重算余额）
                    user.DeductForOrderFunc(totalAmount);

                    // 写订单
                    await orderRepository.AddRangeAsync(orders, ct);

                    // 写一条整批扣款流水（下单前余额 / 下单后余额）
                    string batchNo = Utils.GenerateSerialNo(serialNoPre: "O");
                    await consumeLogRepository.AddRangeAsync([
                        new ConsumeLog(user.Userid, user.UserAmount + totalAmount, user.UserAmount, ConsumeStatus.OrderConsume, batchNo)
                    ], ct);

                    await unitOfWork.SaveChangesAsync(ct);

                    return ApiResult<CreateOrderResult>.Successed(
                        new CreateOrderResult
                        {
                            OrderNos = orders.Select(o => o.OrderNo).ToList(),
                            TotalAmount = totalAmount
                        },
                        orders.Count);
                }, ct);
            }
            catch (ConcurrencyConflictException)
            {
                // 多次重试仍因并发冲突失败，转为友好的业务异常（统一转 400）。
                throw new BusinessException("并发下单冲突，请稍后重试。");
            }
        }

        /// <summary>
        /// 装配当前用户的定价上下文：一次性查出用户单独定价、上级代理的单独定价/加价/总体加价，避免逐条 N+1。
        /// 先 await 取值，再用对象初始化器构造（UserPriceContext 属性为 init-only）。
        /// </summary>
        private async Task<UserPriceContext> BuildPriceContextAsync(TkUser user, List<int> configIds, CancellationToken ct)
        {
            var userCustom = await configRepository.GetUserCustomPricesAsync(user.Userid, configIds, ct);

            Dictionary<int, decimal> agentCustom = new();
            Dictionary<int, decimal> agentMarkup = new();
            bool hasOverall = false;
            int overallPercent = 0;

            if (user.AgentUserid > 0)
            {
                long agentId = user.AgentUserid;
                agentCustom = await configRepository.GetAgentCustomPricesAsync(agentId, configIds, ct);
                agentMarkup = await configRepository.GetAgentMarkupsAsync(agentId, configIds, ct);
                var overall = await configRepository.GetAgentOverallAsync(agentId, ct);
                hasOverall = overall is not null;
                overallPercent = overall?.OverallPercent ?? 0;
            }

            return new UserPriceContext
            {
                HasAgent = user.AgentUserid > 0,
                UserCustom = userCustom,
                AgentCustom = agentCustom,
                AgentMarkup = agentMarkup,
                HasOverall = hasOverall,
                OverallPercent = overallPercent
            };
        }
    }
}
