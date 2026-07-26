using Application.Abstractions;
using Application.Abstractions.Passwords;
using Application.Common.Models;
using Application.Events.Agent.Contracts;
using Application.Events.Agent.Contracts.Commands;
using Application.Events.User.Contracts;
using Application.Events.User.Contracts.Commands;
using Domain.Entities;
using MediatR;
using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Events.Agent
{
    public class CreateChildrenCommandHandler(
        IAgentRepository agentRepository,
        ITkUserRepository tkUserRepository,
        IPasswordHelper passwordHelper,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<CreateChildrenCommand, ApiResult>
    {
        public async Task<ApiResult> Handle(CreateChildrenCommand cmd, CancellationToken ct)
        {
            var agent = await tkUserRepository.GetByIdAsync(cmd.AgentUserid);
            if (agent.IsAgentFnc())
            {
                throw new BusinessException("无代理身份!");
            }
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
}
