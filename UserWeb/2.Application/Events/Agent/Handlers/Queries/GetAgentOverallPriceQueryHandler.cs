using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Common.Models.Response.Agent;
using Application.Events.Agent.Contracts.Queries;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Events.Agent.Handlers.Queries
{
    public class GetAgentOverallPriceQueryHandler(IAgentPricingRepository agentPricingRepository, ICurrentUser currentUser)
        : IRequestHandler<GetAgentOverallPriceQuery, ApiResult<AgentOverallPriceResponse>>
    {
        public async Task<ApiResult<AgentOverallPriceResponse>> Handle(GetAgentOverallPriceQuery request, CancellationToken ct)
        {
            var entity = await agentPricingRepository.GetOverallByUserAsync(currentUser.Userid, ct);
            var percent = entity?.OverallPercent ?? 0;

            return new ApiResult<AgentOverallPriceResponse>
            {
                Code = 200,
                Message = "Success!",
                Data = new AgentOverallPriceResponse { OverallPercent = percent },
                DataTotal = 1
            };
        }
    }
}
