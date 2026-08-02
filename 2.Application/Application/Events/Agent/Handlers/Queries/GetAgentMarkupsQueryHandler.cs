using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Common.Models.Agent;
using Application.Events.Agent.Contracts.Queries;
using Application.Services;
using MediatR;
using Shared.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Events.Agent.Handlers.Queries
{
    public class GetAgentMarkupsQueryHandler(
        IAgentPricingRepository agentPricingRepository,
        IConfigRepository configRepository)
        : IRequestHandler<GetAgentMarkupsQuery, ApiResult<List<AgentMarkupListItem>>>
    {
        public async Task<ApiResult<List<AgentMarkupListItem>>> Handle(GetAgentMarkupsQuery query, CancellationToken ct)
        {
            var (items, total) = await agentPricingRepository.GetMarkupsByAgentAsync(
                query.UserId, query.PageIndex, query.PageSize, query.Keyword, ct);

            if (items.Count == 0)
            {
                return ApiResult<List<AgentMarkupListItem>>.Successed(new List<AgentMarkupListItem>(), 0);
            }

            var configIds = items.Select(x => x.Config.ConfigId).ToList();
            var agentCustom = await configRepository.GetAgentCustomPricesAsync(query.UserId, configIds, ct);

            var list = items.Select(x =>
            {
                var config = x.Config;
                var markup = x.Markup;
                decimal basePrice = agentCustom.TryGetValue(config.ConfigId, out var custom)
                    ? custom
                    : config.ConfigPrice;
                // 下级展示价 = 基础价格 + 加价金额，按单个展示
                decimal childDisplayPrice = basePrice + markup.MarkupAddPrice;

                return new AgentMarkupListItem
                {
                    MarkupId = markup.MarkupId,
                    ConfigId = config.ConfigId,
                    ConfigName = config.ConfigName,
                    ConfigNotice = config.ConfigNotice,
                    ConfigPrice = Utils.RoundToSixDecimals(config.ConfigPrice),
                    BasePrice = Utils.RoundToSixDecimals(basePrice),
                    MarkupAddPrice = Utils.RoundToSixDecimals(markup.MarkupAddPrice),
                    ShowPriceUnit = config.ShowPriceUnit,
                    ChildDisplayPrice = Utils.RoundToSixDecimals(childDisplayPrice),
                    CreateTime = markup.CreateTime
                };
            }).ToList();

            return new ApiResult<List<AgentMarkupListItem>>
            {
                Code = 200,
                Message = "Success!",
                Data = list,
                DataTotal = total
            };
        }
    }
}
