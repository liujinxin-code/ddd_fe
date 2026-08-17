using Application.Common.Models;
using Application.Common.Models.Response.Agent;
using MediatR;

namespace Application.Events.Agent.Contracts.Queries
{
    /// <summary>
    /// 获取代理当前总体加价百分比。
    /// 当前代理用户id 由 ICurrentUser 注入，前台不可伪造。
    /// </summary>
    public record class GetAgentOverallPriceQuery() : IRequest<ApiResult<AgentOverallPriceResponse>>;
}
