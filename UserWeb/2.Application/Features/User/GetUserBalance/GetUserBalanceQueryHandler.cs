using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Features.User;
using MediatR;
using Shared.Exceptions;

namespace Application.Features.User
{
    public class GetUserBalanceQueryHandler(
        ITkUserRepository tkUserRepository,
        ICurrentUser currentUser) : IRequestHandler<GetUserBalanceQuery, ApiResult<UserBalanceResponse>>
    {
        public async Task<ApiResult<UserBalanceResponse>> Handle(GetUserBalanceQuery query, CancellationToken ct)
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

            return ApiResult<UserBalanceResponse>.Successed(
                new UserBalanceResponse(user.UserAmount), 1, "查询成功");
        }
    }
}
