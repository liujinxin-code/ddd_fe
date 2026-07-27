using Application.Abstractions;
using Application.Abstractions.Passwords;
using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Events.User.Contracts.Commands;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Events.User.Handles.Commands
{
    public class RegisterCommandHandler(
     ITkUserRepository tkUserRepository
     , IUnitOfWork unitOfWork
     , IPasswordHelper passwordHelper) : IRequestHandler<RegisterCommand, ApiResult>
    {
        // 固定签名：Handle(请求, 取消令牌)
        public async Task<ApiResult> Handle(RegisterCommand cmd, CancellationToken ct)
        {
            string email = cmd.email.Trim().ToLower();
            string username = cmd.username.Trim().ToLower();
            if (await tkUserRepository.GetEmailExists(email))
            {
                throw new BusinessException("邮箱名已存在，请更换邮箱名");
            }
            if (await tkUserRepository.GetUserNameExists(username))
            {
                throw new BusinessException("用户名已存在，请更换用户名");
            }
            var agent = await tkUserRepository.GetAgentByDomain(cmd.agentDomain);
            string hashPwd = passwordHelper.GeneratePasswordHash(cmd.password);
            var user = new TkUser(email,
          username
               , hashPwd
               , TkUserStatus.Enable
               , agent?.Userid ?? 0
               , 0
               , cmd.agentDomain
               , string.Empty //登录时赋予初始值
               , "前台注册");
            await tkUserRepository.AddAsync(user, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return ApiResult.Successed("注册成功");
        }
    }

}
