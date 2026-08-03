using Application.Abstractions;
using Application.Abstractions.Passwords;
using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Common.Models.Agent;
using Application.Events.Agent.Contracts.Commands;
using MediatR;
using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Events.Agent.Handlers.Commands
{
    public class ResetChildrenPasswordCommandHandler(
        IPasswordHelper passwordHelper,
        ITkUserRepository tkUserRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
           ) : IRequestHandler<ResetChildrenPasswordCommand, ApiResult<ResetChildrenPasswordResponse>>
    {


        public async Task<ApiResult<ResetChildrenPasswordResponse>> Handle(ResetChildrenPasswordCommand request, CancellationToken ct)
        {
            string newPassword = passwordHelper.GenerateRandomPwd();
            string newPasswordHash = passwordHelper.GeneratePasswordHash(newPassword);
            var agent = await tkUserRepository.GetByIdAsync(currentUser.Userid, ct);
            if (agent == null) throw new BusinessException("代理不存在");
            var children = await tkUserRepository.GetByIdAsync(request.ChildrenUserid, ct);
            if (children == null) throw new BusinessException("用户不存在");

            agent!.ResetChildrenPasswordFunc(children!, newPasswordHash);
            //TODO 增加数据库日志
            await unitOfWork.SaveChangesAsync(ct);
            return ApiResult<ResetChildrenPasswordResponse>.Successed(new ResetChildrenPasswordResponse(newPassword));
        }
    }
}
