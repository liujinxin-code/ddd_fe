using MediatR;
using Application.Common.Models;
using System;

namespace Application.Features.Agent
{
    /// <summary>
    /// 代理新增 / 修改自己名下某 config 的加价金额（tk_price_agent_markup）。
    /// 同一 (config_id, agent_userid) 语义唯一：存在则修改，否则新增。
    /// 当前代理用户id 由 ICurrentUser 注入，前台不可伪造。
    /// </summary>
    public record class UpsertAgentMarkupCommand(int ConfigId, decimal MarkupAddPrice) : IRequest<Unit>;
}
