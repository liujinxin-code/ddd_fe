using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Common.Models.Request.Order;
using Application.Common.Models.Response.Order;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class OrderRepository(AppDbContext appDbContext) : IRepository<TkOrder>, IOrderRepository
    {
        public Task<bool> AddAsync(TkOrder entity, CancellationToken ct = default)
            => Task.FromResult(appDbContext.TkOrders.Add(entity) != null);

        public Task AddRangeAsync(IEnumerable<TkOrder> orders, CancellationToken ct = default)
            => appDbContext.TkOrders.AddRangeAsync(orders, ct);

        /// <summary>
        /// 批量落订单并显式落评论。评论已按 order_no 显式赋值（订单分表后不再依赖级联/导航回填），
        /// 故评论需要单独 Add 到 TkComments。两个集合由调用方在同一 SaveChanges 内原子提交。
        /// </summary>
        public async Task AddOrdersWithCommentsAsync(IEnumerable<TkOrder> orders, IEnumerable<TkComment> comments, CancellationToken ct = default)
        {
            await appDbContext.TkOrders.AddRangeAsync(orders, ct);
            await appDbContext.TkComments.AddRangeAsync(comments, ct);
        }

        public Task<TkOrder?> GetByIdAsync(long id, CancellationToken ct = default)
            => appDbContext.TkOrders.FirstOrDefaultAsync(t => t.OrderId == id, ct);

        public Task<TkOrder?> GetByIdAsNoTrackingAsync(long id, CancellationToken ct = default)
            => appDbContext.TkOrders.AsNoTracking().FirstOrDefaultAsync(t => t.OrderId == id, ct);

        public void Update(TkOrder entity, CancellationToken ct = default)
            => appDbContext.TkOrders.Update(entity);

        /// <summary>
        /// 订单列表读模型：一次 SQL 左连出平台名/业务类型名/业务名，避免逐条回查。
        /// 使用左连是因为配置可能被下架清理，此时订单本身仍需可见（名称回退为空串）。
        /// </summary>
        public async Task<(IReadOnlyList<OrderResponse> Items, int Total)> GetPagedByUserAsync(
            long userId,
            int orderState,
            string? keyword,
            int pageIndex,
            int pageSize,
            string? sortField,
            bool sortDesc,
            CancellationToken ct = default)
        {
            var orders = appDbContext.TkOrders.AsNoTracking().Where(o => o.Userid == userId);

            if (orderState > 0)
            {
                var state = (Domain.Enums.OrderState)orderState;
                orders = orders.Where(o => o.OrderState == state);
            }

            // 订单号 / 下单链接 模糊检索，LIKE 由 EF 参数化，无注入风险。
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                orders = orders.Where(o => EF.Functions.Like(o.OrderNo, $"%{kw}%")
                                        || EF.Functions.Like(o.OrderLink, $"%{kw}%"));
            }

            int total = await orders.CountAsync(ct);
            if (total == 0)
            {
                return (new List<OrderResponse>(), 0);
            }

            orders = ApplyOrderSorting(orders, sortField, sortDesc);

            var query =
                from o in orders
                join c in appDbContext.TkConfigs.AsNoTracking() on o.ConfigId equals c.ConfigId into configGroup
                from c in configGroup.DefaultIfEmpty()
                join p in appDbContext.TkPlatforms.AsNoTracking() on c.PlatformId equals p.PlatformId into platformGroup
                from p in platformGroup.DefaultIfEmpty()
                join s in appDbContext.TkPlatformSubs.AsNoTracking() on c.SubPlatformId equals s.SubPlatformId into subGroup
                from s in subGroup.DefaultIfEmpty()
                select new OrderResponse
                {
                    ConfigId = o.ConfigId,
                    OrderNo = o.OrderNo,
                    OrderState = (int)o.OrderState,
                    OrderLink = o.OrderLink,
                    PlatformName = p.PlatformName ?? string.Empty,
                    SubPlatformName = s.SubPlatformName ?? string.Empty,
                    ConfigName = c.ConfigName ?? string.Empty,
                    OrderAmount = o.OrderAmount,
                    Quantity = o.Quantity,
                    SuccessQuantity = o.SuccessQuantity,
                    BeginQuantity = o.BeginQuantity,
                    RefundAmount = o.RefundAmount,
                    CreateTime = o.CreateTime,
                    JsonTemplate = c == null ? 0 : (int)c.JsonTemplate,
                    // 评论业务下单时提交的评论内容（未软删除）；非评论业务为空集合。
                    // 注意：此处不能对子查询加 .OrderBy(cc => cc.CommentId) —— ShardingCore 跨分片归并排序
                    // 会错误地把嵌套子查询的 OrderBy 当成归并排序键，而最终投影类型 OrderResponse 上并无
                    // CommentId 属性，导致 "property:[CommentId] not in type" 异常。CommentId 是自增主键，
                    // InnoDB 对简单 WHERE 通常按主键顺序返回，顺序即提交顺序，无需显式排序。
                    Comments = appDbContext.TkComments
                        .Where(cc => cc.OrderNo == o.OrderNo)
                        .Select(cc => cc.CommentContent)
                        .ToList()
                };

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        /// <summary>
        /// 仅允许白名单内的字段排序，避免任意列名导致 EF 翻译失败或注入。缺省按下单时间倒序。
        /// </summary>
        private static IQueryable<TkOrder> ApplyOrderSorting(IQueryable<TkOrder> query, string? sortField, bool sortDesc)
        {
            switch ((sortField ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "orderamount":
                    return sortDesc ? query.OrderByDescending(o => o.OrderAmount) : query.OrderBy(o => o.OrderAmount);
                case "quantity":
                    return sortDesc ? query.OrderByDescending(o => o.Quantity) : query.OrderBy(o => o.Quantity);
                case "orderstate":
                    return sortDesc ? query.OrderByDescending(o => o.OrderState) : query.OrderBy(o => o.OrderState);
                case "createtime":
                    return sortDesc ? query.OrderByDescending(o => o.CreateTime) : query.OrderBy(o => o.CreateTime);
                default:
                    return query.OrderByDescending(o => o.CreateTime);
            }
        }
    }
}
