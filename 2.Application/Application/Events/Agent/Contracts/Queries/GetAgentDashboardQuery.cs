using Application.Common.Models;
using Application.Common.Models.Agent;
using MediatR;

namespace Application.Events.Agent.Contracts.Queries
{
    /// <summary>
    /// 获取代理管理页顶部仪表盘数据。
    /// AgentUserid 由控制器从当前登录代理注入，前台不可伪造。
    /// 返回用户余额、代理余额、下级用户启用数/总数。
    /// </summary>
    public record class GetAgentDashboardQuery(long AgentUserid = 0)
        : IRequest<ApiResult<AgentDashboardItem>>
    {
    }
}
