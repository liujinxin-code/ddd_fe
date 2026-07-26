using Application.Abstractions;
using Domain.Entities;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;
using Shared.Utilitys;
using Shared.Exceptions;
using Application.Abstractions.Auth;
using Application.Abstractions.Passwords;
using Application.Common.Models;
using Application.Events.User.Contracts;
using Application.Events.User.Contracts.Commands;

namespace Application.Events.User
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


    public class LogoutCommandHandler(ITokenCacheService tokenCacheService
   ) : IRequestHandler<LogoutCommand, ApiResult>
    {

        public async Task<ApiResult> Handle(LogoutCommand args, CancellationToken ct)
        {
            await tokenCacheService.RemoveTokenAsync(args.Jti, args.Userid);
            return ApiResult.Successed("退出成功");
        }
    }
}
