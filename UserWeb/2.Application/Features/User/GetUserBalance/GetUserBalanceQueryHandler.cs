using Application.Abstractions.Repositories;
using Application.Common.Models;

using Application.Features.User;
using MediatR;
using Shared.Exceptions;

namespace Application.Features.User
{
    public class GetUserBalanceQueryHandler(
        ITkUserRepository tkUserRepository,
        ICurrentUser currentUser) : IRequestHandler<GetUserBalanceQuery, UserBalanceResponse>
    {
        public async Task<UserBalanceResponse> Handle(GetUserBalanceQuery query, CancellationToken ct)
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

            return new UserBalanceResponse(user.UserAmount);
        }
    }
}
