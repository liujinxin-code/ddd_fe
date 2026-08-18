using Application.Common.Models;
using MediatR;

namespace Application.Features.Agent
{
    /// <summary>
    /// 代理将代理收益余额提取到个人用户余额。
    /// 当前代理用户id 由 ICurrentUser 注入，前台不可伪造。
    /// </summary>
    /// <param name="Amount">提取金额</param>
    public record class WithdrawAgentAmountCommand(decimal Amount) : IRequest<Unit>;
}
