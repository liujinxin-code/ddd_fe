using Application.Abstractions;
using Application.Abstractions.Auth;
using Application.Abstractions.Passwords;
using Application.Common.Models;
using Application.Common.Models.User;
using Application.User.Contracts;
using Application.User.Contracts.Commands;
using Application.User.Contracts.Queries;
using Domain.Entities;
using Domain.Enums;
using Mapster;
using MediatR;
using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.User
{
    public class LoginQueryHandler(
        ITkUserRepository tkUserRepository
        , IPasswordHelper passwordHelper
        , IJwtHelper jwtHelper
        , ITokenCacheService tokenCacheService) : IRequestHandler<LoginQuery, ApiResult<LoginResponse>>
    {
        // 固定签名：Handle(请求, 取消令牌)
        public async Task<ApiResult<LoginResponse>> Handle(LoginQuery query, CancellationToken ct)
        {
            string name = query.name.Trim().ToLower();
            var user = await tkUserRepository.GetUserByUserNameOrEmailAsync(query.name, ct);
            if (user == null)
                throw new BusinessException("账户或密码错误");
            string password = passwordHelper.GeneratePasswordHash(query.password);
            if (user.Password != password)
                throw new BusinessException("账户或密码错误");
            (string, string) tokenRes = jwtHelper.GenerateToken(user.Userid, user.Username, [user.IsAgentFnc() ? "User.Agent" : "User"]);

            await tokenCacheService.SetTokenAsync(tokenRes.Item2, user.Userid, user.SignleClient);

            var response = new LoginResponse
            {
                Token = tokenRes.Item1,
                User = user.Adapt<LoginUserResponse>()
            };
            return ApiResult<LoginResponse>.Successed(response);

        }
    }
}
