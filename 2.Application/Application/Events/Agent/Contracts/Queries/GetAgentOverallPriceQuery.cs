using Application.Common.Models;
using Application.Common.Models.Agent;
using MediatR;

namespace Application.Events.Agent.Contracts.Queries
{
    /// <summary>
    /// 获取代理当前总体加价百分比。
    /// UserId 由控制器从当前登录用户注入，前台不可伪造。
    /// </summary>
    public record class GetAgentOverallPriceQuery(long UserId = 0) : IRequest<ApiResult<AgentOverallPriceItem>>;
}
