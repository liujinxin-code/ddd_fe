using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Common.Models.Agent;
using Application.Events.Agent.Contracts.Queries;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Events.Agent.Handlers.Queries
{
    public class GetAgentOverallPriceQueryHandler(IAgentPricingRepository agentPricingRepository)
        : IRequestHandler<GetAgentOverallPriceQuery, ApiResult<AgentOverallPriceItem>>
    {
        public async Task<ApiResult<AgentOverallPriceItem>> Handle(GetAgentOverallPriceQuery request, CancellationToken ct)
        {
            var entity = await agentPricingRepository.GetOverallByUserAsync(request.UserId, ct);
            var percent = entity?.OverallPercent ?? 0;

            return new ApiResult<AgentOverallPriceItem>
            {
                Code = 200,
                Message = "Success!",
                Data = new AgentOverallPriceItem { OverallPercent = percent },
                DataTotal = 1
            };
        }
    }
}
