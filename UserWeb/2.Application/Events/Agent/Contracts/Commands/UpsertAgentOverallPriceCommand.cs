using Application.Common.Models;
using MediatR;
using System;

namespace Application.Events.Agent.Contracts.Commands
{
    /// <summary>
    /// 代理新增 / 修改自己的总体加价百分比（tk_price_overall，每代理仅一条）。
    /// 首次为新增，之后为修改（按 UserId 唯一）。
    /// 当前代理用户id 由 ICurrentUser 注入，前台不可伪造。
    /// </summary>
    public record class UpsertAgentOverallPriceCommand(int OverallPercent) : IRequest<ApiResult>;
}
