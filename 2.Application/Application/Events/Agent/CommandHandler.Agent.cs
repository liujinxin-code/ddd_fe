using Application.Abstractions;
using Application.Abstractions.Passwords;
using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Events.Agent.Contracts;
using Application.Events.Agent.Contracts.Commands;
using Application.Events.User.Contracts.Commands;
using Domain.Entities;
using MediatR;
using Shared.Exceptions;
using Shared.Utilitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Events.Agent
{
    public class CreateChildrenCommandHandler(
        ITkUserRepository tkUserRepository,
        IPasswordHelper passwordHelper,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<CreateChildrenCommand, ApiResult>
    {
        public async Task<ApiResult> Handle(CreateChildrenCommand cmd, CancellationToken ct)
        {
            var agent = await tkUserRepository.GetByIdAsync(cmd.AgentUserid);
            if (agent == null) throw new BusinessException("代理不存在");
            agent.RequiredAgentFunc();
            string email = cmd.Email.Trim().ToLower();
            string username = cmd.Username.Trim().ToLower();
            if (await tkUserRepository.GetUserNameExists(username, ct))
            {
                throw new BusinessException("用户名已存在");
            }
            if (await tkUserRepository.GetEmailExists(email, ct))
            {
                throw new BusinessException("邮箱号已存在");
            }

            var children = new TkUser(email, username, passwordHelper.GeneratePasswordHash(cmd.Password),
                Domain.Enums.TkUserStatus.Enable, agent.Userid, 0, agent.AgentDomain, string.Empty, agent.Username);
            await tkUserRepository.AddAsync(children, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return ApiResult.Successed();
        }
    }

    public class TransferUserAmountCommandHandler(
           ITkUserRepository tkUserRepository
        , IConsumeLogRepository consumeLogRepository,
           IUnitOfWork unitOfWork
        ) : IRequestHandler<TransferUserAmountCommand, ApiResult>
    {
        public async Task<ApiResult> Handle(TransferUserAmountCommand cmd, CancellationToken ct)
        {
            // 整段操作（加载 - 校验 - 改双方余额 - 记录两条流水 - 落库）放进乐观并发重试，
            // 与 tk_user.user_version 令牌配合：若转账过程中对方正在下单等并发改了同一用户余额，
            // 后提交者会触发 DbUpdateConcurrencyException，重试时重新加载最新余额并重算，避免丢更新。
            try
            {
                await unitOfWork.ExecuteWithRetryAsync(async () =>
                {
                    var agent = await tkUserRepository.GetByIdAsync(cmd.AgentUserid);
                    if (agent == null) throw new BusinessException("代理不存在");

                    var children = await tkUserRepository.GetByIdAsync(cmd.ChildrenUserid);
                    if (children == null) throw new BusinessException("用户不存在");
                    var agentBefore = agent.UserAmount;
                    var childBefore = children.UserAmount;

                    agent.TransferAmountToChildrenFunc(cmd.transferAmount, children);
                    string serailNo = Utils.GenerateSerialNo(serailNoPre: "C");
                    await consumeLogRepository.AddRangeAsync([
                     new ConsumeLog(agent.Userid,agentBefore,agent.UserAmount, Domain.Enums.ConsumeStatus.AgentTransferOut, serailNo),
                 new ConsumeLog(children.Userid,childBefore,children.UserAmount, Domain.Enums.ConsumeStatus.AgentTransferIn,serailNo ),
                    ], ct);
                    await unitOfWork.SaveChangesAsync(ct);
                }, ct);
            }
            catch (ConcurrencyConflictException)
            {
                // 多次重试仍因并发冲突失败，转为友好的业务异常交由上层处理。
                throw new BusinessException("并发更新冲突，转账未成功，请稍后重试。");
            }
            return ApiResult.Successed();
        }
    }
}
