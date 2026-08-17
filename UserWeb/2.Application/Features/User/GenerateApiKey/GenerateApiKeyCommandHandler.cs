using Application.Abstractions;
using Application.Abstractions.Auth;
using Application.Abstractions.Passwords;
using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Features.User;
using MediatR;
using Shared.Exceptions;

namespace Application.Features.User
{
    public class GenerateApiKeyCommandHandler(
        ITkUserRepository tkUserRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IJwtHelper jwtHelper) : IRequestHandler<GenerateApiKeyCommand, ApiResult<GenerateApiKeyResponse>>
    {
        public async Task<ApiResult<GenerateApiKeyResponse>> Handle(GenerateApiKeyCommand cmd, CancellationToken ct)
        {
            if (currentUser.Userid <= 0)
            {
                throw new BusinessException("用户未登录或身份无效");
            }

            var user = await tkUserRepository.GetByIdAsync(currentUser.Userid, ct);
            if (user == null)
            {
                throw new BusinessException("用户不存在");
            }

            var roles = user.IsAgentFnc() ? new[] { "User.Agent" } : new[] { "User" };
            // 20 年长期 JWT，client_type=API
            var (apiKey, _) = jwtHelper.GenerateToken(user.Userid, user.Username, roles, "API", 365 * 20);

            user.SetApiKeyFunc(apiKey);
            await unitOfWork.SaveChangesAsync(ct);

            return ApiResult<GenerateApiKeyResponse>.Successed(new GenerateApiKeyResponse(apiKey), 1, "API Key 已生成");
        }
    }

    public class ViewApiKeyCommandHandler(
        ITkUserRepository tkUserRepository,
        IPasswordHelper passwordHelper,
        ICurrentUser currentUser) : IRequestHandler<ViewApiKeyCommand, ApiResult<GenerateApiKeyResponse>>
    {
        public async Task<ApiResult<GenerateApiKeyResponse>> Handle(ViewApiKeyCommand cmd, CancellationToken ct)
        {
            if (currentUser.Userid <= 0)
            {
                throw new BusinessException("用户未登录或身份无效");
            }
            if (string.IsNullOrWhiteSpace(cmd.Password))
            {
                throw new BusinessException("请输入登录密码");
            }

            var user = await tkUserRepository.GetByIdAsync(currentUser.Userid, ct);
            if (user == null)
            {
                throw new BusinessException("用户不存在");
            }

            if (!passwordHelper.VerifyPassword(cmd.Password, user.Password))
            {
                throw new BusinessException("密码不正确");
            }

            return ApiResult<GenerateApiKeyResponse>.Successed(
                new GenerateApiKeyResponse(user.ApiKey ?? string.Empty), 1, "API Key 已获取");
        }
    }
}
