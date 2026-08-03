using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Events.Agent.Contracts.Commands;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Shared.Exceptions;
using Shared.Utilities;

namespace Application.Events.Agent.Handlers.Commands
{
    public class WithdrawAgentAmountCommandHandler(
            ITkUserRepository tkUserRepository,
            IConsumeLogRepository consumeLogRepository,
            IUnitOfWork unitOfWork
         ) : IRequestHandler<WithdrawAgentAmountCommand, ApiResult>
    {
        public async Task<ApiResult> Handle(WithdrawAgentAmountCommand cmd, CancellationToken ct)
        {
            try
            {
                await unitOfWork.ExecuteWithRetryAsync(async () =>
                {
                    var agent = await tkUserRepository.GetByIdAsync(cmd.AgentUserId);
                    if (agent == null) throw new BusinessException("代理不存在");

                    var userBefore = agent.UserAmount;
                    agent.WithdrawAgentAmountToUserAmountFunc(cmd.Amount);

                    string serialNo = Utils.GenerateSerialNo(serialNoPre: "W");
                    await consumeLogRepository.AddAsync(
                        new ConsumeLog(agent.Userid, userBefore, agent.UserAmount, ConsumeStatus.AgentWithdraw, serialNo),
                        ct);
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
