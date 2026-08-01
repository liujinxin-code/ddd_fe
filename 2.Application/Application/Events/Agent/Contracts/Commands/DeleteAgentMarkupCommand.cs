using Application.Common.Models;
using MediatR;
using System;

namespace Application.Events.Agent.Contracts.Commands
{
    /// <summary>
    /// 代理删除自己名下某 config 的加价记录（tk_price_agent_markup）。
    /// AgentUserId 由控制器从当前登录用户注入（CurrentUser.Userid），前台不可伪造。
    /// </summary>
    public record class DeleteAgentMarkupCommand(int ConfigId, long AgentUserId = 0) : IRequest<ApiResult>;
}
