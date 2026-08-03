using Application.Common.Models;
using MediatR;

namespace Application.Events.Agent.Contracts.Commands
{
    /// <summary>
    /// 代理将代理收益余额提取到个人用户余额。
    /// </summary>
    /// <param name="AgentUserId">当前代理用户id（由 Controller 从 CurrentUser 注入）</param>
    /// <param name="Amount">提取金额</param>
    public record class WithdrawAgentAmountCommand(long AgentUserId, decimal Amount) : IRequest<ApiResult>;
}
