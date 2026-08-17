using Application.Common.Models;
using MediatR;
using System;

namespace Application.Events.Agent.Contracts.Commands
{
    /// <summary>
    /// 代理删除自己名下某 config 的加价记录（tk_price_agent_markup）。
    /// 当前代理用户id 由 ICurrentUser 注入，前台不可伪造。
    /// </summary>
    public record class DeleteAgentMarkupCommand(int ConfigId) : IRequest<ApiResult>;
}
