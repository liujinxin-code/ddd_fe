using Application.Common.Models;
using MediatR;
using System;

namespace Application.Events.Agent.Contracts.Commands
{
    /// <summary>
    /// 代理新增 / 修改自己名下某 config 的加价金额（tk_price_agent_markup）。
    /// 同一 (config_id, agent_userid) 语义唯一：存在则修改，否则新增。
    /// AgentUserId 由控制器从当前登录用户注入（CurrentUser.Userid），前台不可伪造。
    /// </summary>
    public record class UpsertAgentMarkupCommand(int ConfigId, decimal MarkupAddPrice, long AgentUserId = 0) : IRequest<ApiResult>;
}
