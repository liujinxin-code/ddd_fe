using Application.Features.Agent.Models;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Agent
{
    /// <summary>
    /// 获取代理管理页顶部仪表盘数据。
    /// 返回用户余额、代理余额、下级用户启用数/总数。
    /// 当前代理用户id 由 ICurrentUser 注入，前台不可伪造。
    /// </summary>
    public record class GetAgentDashboardQuery()
        : IRequest<AgentDashboardResponse?>;
}
