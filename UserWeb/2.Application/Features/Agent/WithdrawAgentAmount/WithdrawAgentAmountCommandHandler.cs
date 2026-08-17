using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Features.Agent;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Shared.Exceptions;
using Shared.Utilities;

namespace Application.Features.Agent
{
    public class WithdrawAgentAmountCommandHandler(
            ITkUserRepository tkUserRepository,
            IConsumeLogRepository consumeLogRepository,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
         ) : IRequestHandler<WithdrawAgentAmountCommand, ApiResult>
    {
        public async Task<ApiResult> Handle(WithdrawAgentAmountCommand cmd, CancellationToken ct)
        {
            if (!currentUser.IsAuthenticated || currentUser.Userid <= 0)
                return ApiResult.UnAuth();

            try
            {
                await unitOfWork.ExecuteWithRetryAsync(async () =>
                {
                    var agent = await tkUserRepository.GetByIdAsync(currentUser.Userid);
                    if (agent == null) throw new BusinessException("代理不存在");

                    var userBefore = agent.UserAmount;      // 个人用户余额（提现入账前）
                    var agentBefore = agent.AgentAmount;     // 代理收益余额（提现转出前）
                    agent.WithdrawAgentAmountToUserAmountFunc(cmd.Amount);

                    string serialNo = Utils.GenerateSerialNo(serialNoPre: "W");
                    await consumeLogRepository.AddRangeAsync([
                        // 来源侧：代理收益余额减少
                        new ConsumeLog(agent.Userid, agentBefore, agent.AgentAmount, ConsumeStatus.AgentWithdrawOut, serialNo),
                        // 入账侧：个人用户余额增加
                        new ConsumeLog(agent.Userid, userBefore, agent.UserAmount, ConsumeStatus.AgentWithdraw, serialNo),
                    ], ct);
                    await unitOfWork.SaveChangesAsync(ct);
                }, ct);
            }
            catch (ConcurrencyConflictException)
            {
                throw new BusinessException("并发更新冲突，提取未成功，请稍后重试。");
            }
            return ApiResult.Successed();
        }
    }
}
