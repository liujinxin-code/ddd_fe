using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Common.Models.Response.Agent;
using Application.Events.Agent.Contracts.Queries;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Events.Agent.Handlers.Queries
{
    public class GetAgentDashboardQueryHandler(
        ITkUserRepository tkUserRepository,
        ICurrentUser currentUser)
        : IRequestHandler<GetAgentDashboardQuery, ApiResult<AgentDashboardResponse>>
    {
        public async Task<ApiResult<AgentDashboardResponse>> Handle(GetAgentDashboardQuery query, CancellationToken ct)
        {
            var user = await tkUserRepository.GetByIdAsync(currentUser.Userid, ct);
            if (user == null)
            {
                return new ApiResult<AgentDashboardResponse>
                {
                    Code = 404,
                    Message = "用户不存在。",
                    Data = null
                };
            }

            var (enabledCount, totalCount) = await tkUserRepository.GetChildrenStatsAsync(currentUser.Userid, ct);

            var item = new AgentDashboardResponse
            {
                UserAmount = user.UserAmount,
                AgentAmount = user.AgentAmount,
                EnabledChildrenCount = enabledCount,
                TotalChildrenCount = totalCount
            };

            return new ApiResult<AgentDashboardResponse>
            {
                Code = 200,
                Message = "Success!",
                Data = item
            };
        }
    }
}
