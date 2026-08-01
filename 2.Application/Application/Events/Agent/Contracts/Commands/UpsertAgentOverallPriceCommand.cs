using Application.Common.Models;
using MediatR;
using System;

namespace Application.Events.Agent.Contracts.Commands
{
    /// <summary>
    /// 代理新增 / 修改自己的总体加价百分比（tk_price_overall，每代理仅一条）。
    /// 首次为新增，之后为修改（按 UserId 唯一）。
    /// UserId 由控制器从当前登录用户注入（CurrentUser.Userid），前台不可伪造。
    /// </summary>
    public record class UpsertAgentOverallPriceCommand(int OverallPercent, long UserId = 0) : IRequest<ApiResult>;
}
