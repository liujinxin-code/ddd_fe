using Application.Abstractions.Repositories;
using Application.Common.Models;

using Application.Features.Agent.Models;
using Application.Features.Agent;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Agent
{
    public class GetAgentOverallPriceQueryHandler(IAgentPricingRepository agentPricingRepository, ICurrentUser currentUser)
        : IRequestHandler<GetAgentOverallPriceQuery, AgentOverallPriceResponse>
    {
        public async Task<AgentOverallPriceResponse> Handle(GetAgentOverallPriceQuery request, CancellationToken ct)
        {
            var entity = await agentPricingRepository.GetOverallByUserAsync(currentUser.Userid, ct);
            var percent = entity?.OverallPercent ?? 0;

            return new AgentOverallPriceResponse { OverallPercent = percent };
        }
    }
}
