using Application.Abstractions.Auth;
using Application.Abstractions.Passwords;
using Application.Common.Models;
using Application.Common.Models.User;
using Application.Events.User.Contracts;
using Application.Events.User.Contracts.Queries;
using Domain.Entities;
using Mapster;
using MediatR;
using Shared.Exceptions;

namespace Application.Events.User
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

    public class GetUserInfoQueryHandler(ITkUserRepository tkUserRepository) : IRequestHandler<GetUserInfoQuery, ApiResult<UserInfoResponse>>
    {
        public async Task<ApiResult<UserInfoResponse>> Handle(GetUserInfoQuery query, CancellationToken ct)
        {
            TkUser? user = await tkUserRepository.GetByIdAsync(query.Userid, ct);
            if (user == null)
            {
                throw new BusinessException("用户不存在");
            }
            var response = user.Adapt<UserInfoResponse>();

            return ApiResult<UserInfoResponse>.Successed(response);

        }
    }
}
