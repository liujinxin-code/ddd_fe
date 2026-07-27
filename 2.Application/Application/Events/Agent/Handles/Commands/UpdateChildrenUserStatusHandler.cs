using Application.Abstractions;
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

namespace Application.Events.Agent.Handles.Commands
{


    public class UpdateChildrenUserStatusHandler(
        ITkUserRepository tkUserRepository,
        IUnitOfWork unitOfWork
          ) : IRequestHandler<UpdateChildrenUserStatusCommand, ApiResult>
    {


        public async Task<ApiResult> Handle(UpdateChildrenUserStatusCommand request, CancellationToken ct)
        {
            var agent = await tkUserRepository.GetByIdAsync(request.AgentUserid);
            if (agent == null) throw new BusinessException("代理不存在");
            var children = await tkUserRepository.GetByIdAsync(request.ChildrenUserid);
            if (children == null) throw new BusinessException("用户不存在");

            agent.UpdateChildrenStatusFunc(children, request.UserStatus);
            //TODO 增加数据库日志
            await unitOfWork.SaveChangesAsync(ct);
            return ApiResult.Successed();

        }
    }
}
