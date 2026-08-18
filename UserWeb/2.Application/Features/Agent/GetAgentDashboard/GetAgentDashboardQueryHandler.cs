using Application.Abstractions;
using Application.Common.Models;
using Application.Abstractions.Repositories;

using Application.Features.Agent.Models;
using Application.Features.Agent;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Agent
{
    public class GetAgentDashboardQueryHandler(
        ITkUserRepository tkUserRepository,
        ICurrentUser currentUser)
        : IRequestHandler<GetAgentDashboardQuery, AgentDashboardResponse?>
    {
        public async Task<AgentDashboardResponse?> Handle(GetAgentDashboardQuery query, CancellationToken ct)
        {
            var user = await tkUserRepository.GetByIdAsync(currentUser.Userid, ct);
            if (user == null)
            {
                return null;
            }

            var (enabledCount, totalCount) = await tkUserRepository.GetChildrenStatsAsync(currentUser.Userid, ct);

            var item = new AgentDashboardResponse
            {
                UserAmount = user.UserAmount,
                AgentAmount = user.AgentAmount,
                EnabledChildrenCount = enabledCount,
                TotalChildrenCount = totalCount
            };

            return item;
        }
    }
}
