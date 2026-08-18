using Application.Abstractions.Repositories;
using Application.Common.Models;

using Application.Features.Agent.Models;
using Application.Features.Agent;
using Application.Services;
using MediatR;
using Shared.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Agent
{
    public class GetAgentMarkupsQueryHandler(
        IAgentPricingRepository agentPricingRepository,
        IConfigRepository configRepository,
        ICurrentUser currentUser)
        : IRequestHandler<GetAgentMarkupsQuery, PagedResult<AgentMarkupResponse>>
    {
        public async Task<PagedResult<AgentMarkupResponse>> Handle(GetAgentMarkupsQuery query, CancellationToken ct)
        {
            var (items, total) = await agentPricingRepository.GetMarkupsByAgentAsync(
                currentUser.Userid, query.PageIndex, query.PageSize, query.Keyword, ct);

            if (items.Count == 0)
            {
                return new PagedResult<AgentMarkupResponse>(new List<AgentMarkupResponse>(), 0);
            }

            var configIds = items.Select(x => x.Config.ConfigId).ToList();
            var agentCustom = await configRepository.GetAgentCustomPricesAsync(currentUser.Userid, configIds, ct);

            var list = items.Select(x =>
            {
                var config = x.Config;
                var markup = x.Markup;
                decimal basePrice = agentCustom.TryGetValue(config.ConfigId, out var custom)
                    ? custom
                    : config.ConfigPrice;
                // 下级展示价 = 基础价格 + 加价金额，按单个展示
                decimal childDisplayPrice = basePrice + markup.MarkupAddPrice;

                return new AgentMarkupResponse
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

            return new PagedResult<AgentMarkupResponse>(list, total);
        }
    }
}
