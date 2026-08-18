using Application.Abstractions.Repositories;
using Shared.Exceptions;
using Application.Common.Models;

using Application.Features.Config.Models;
using Application.Features.Config;
using Application.Services;
using Domain.Entities;
using MediatR;
using Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Config
{
    public class GetConfigsQueryHandler(
        IConfigRepository configRepository,
        ITkUserRepository userRepository,
        ICurrentUser currentUser)
        : IRequestHandler<GetConfigsQuery, PagedResult<ConfigResponse>>
    {
        public async Task<PagedResult<ConfigResponse>> Handle(GetConfigsQuery query, CancellationToken ct)
        {
            if (!currentUser.IsAuthenticated || currentUser.Userid <= 0)
            {
                throw new UnauthorizedDomainException();
            }

            // 解析排序：格式 "字段 [asc|desc]"，缺省按 config_sort 升序。
            string sortField;
            bool sortDesc;
            if (string.IsNullOrWhiteSpace(query.Sorting))
            {
                sortField = "configsort";
                sortDesc = false;
            }
            else
            {
                var parts = query.Sorting.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                sortField = parts[0];
                sortDesc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
            }

            // 查询该业务类型下前台可见的业务配置（分页）。
            var (configs, total) = await configRepository.GetConfigsAsync(
                query.PlatformId, query.SubPlatformId, query.PageIndex, query.PageSize, sortField, sortDesc, query.Keyword, ct);

            if (configs.Count == 0)
            {
                return new PagedResult<ConfigResponse>(new List<ConfigResponse>(), 0);
            }

            // 加载当前用户，确定其是否有上级代理，从而决定定价路径。
            var user = await userRepository.GetByIdAsNoTrackingAsync(currentUser.Userid, ct);
            if (user is null)
            {
                return new PagedResult<ConfigResponse>(new List<ConfigResponse>(), 0);
            }

            long agentUserId = user.AgentUserid;
            bool hasAgent = agentUserId > 0;

            // 一次性装配当前用户的定价上下文（批量查询，避免 N+1）。
            var configIds = configs.Select(c => c.ConfigId).ToList();
            var userCustom = await configRepository.GetUserCustomPricesAsync(currentUser.Userid, configIds, ct);

            Dictionary<int, decimal> agentCustom = new();
            Dictionary<int, decimal> agentMarkup = new();
            TkPriceOverall? overall = null;
            if (hasAgent)
            {
                agentCustom = await configRepository.GetAgentCustomPricesAsync(agentUserId, configIds, ct);
                agentMarkup = await configRepository.GetAgentMarkupsAsync(agentUserId, configIds, ct);
                overall = await configRepository.GetAgentOverallAsync(agentUserId, ct);
            }

            var priceContext = new UserPriceContext
            {
                HasAgent = hasAgent,
                UserCustom = userCustom,
                AgentCustom = agentCustom,
                AgentMarkup = agentMarkup,
                HasOverall = overall != null,
                OverallPercent = overall?.OverallPercent ?? 0
            };

            // 计算每个业务对当前用户的最终单价与展示价。
            var items = configs.Select(c =>
            {
                var unitPrice = ConfigPriceCalculator.CalculateUnitPrice(c, priceContext);
                var displayPrice = c.ShowPriceUnit > 0 ? c.ShowPriceUnit * unitPrice : unitPrice;
                // 价格统一四舍五入保留 6 位小数（半进位）。
                displayPrice = Utils.RoundToSixDecimals(displayPrice);
                return new ConfigResponse
                {
                    ConfigId = c.ConfigId,
                    ConfigName = c.ConfigName,
                    ConfigNotice = c.ConfigNotice,
                    UnitPrice = unitPrice,
                    ShowPriceUnit = c.ShowPriceUnit,
                    DisplayPrice = displayPrice,
                    MinQuantity = c.MinQuantity,
                    MaxQuantity = c.MaxQuantity,
                    OrderUnit = c.OrderUnit,
                    JsonTemplate = c.JsonTemplate,
                    ConfigSort = c.ConfigSort
                };
            }).ToList();

            // 返回中性分页载体 PagedResult，真实总条数 total 由 TotalCount 携带；
            // HTTP 边缘层（ApiPaged）会显式构造信封，避免 IList 被 ApiResult.Successed 按 Count 覆盖总条数。
            return new PagedResult<ConfigResponse>(items, total);
        }
    }
}
