using Application.Abstractions.Repositories;
using Application.Common.Models;

using Application.Features.User.Models;
using Application.Features.User;
using Domain.Entities;
using Mapster;
using MediatR;
using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.User
{
    public class GetUserInfoQueryHandler(ITkUserRepository tkUserRepository, ICurrentUser currentUser) : IRequestHandler<GetUserInfoQuery, UserInfoResponse>
    {
        public async Task<UserInfoResponse> Handle(GetUserInfoQuery query, CancellationToken ct)
        {
            TkUser? user = await tkUserRepository.GetByIdAsync(currentUser.Userid, ct);
            if (user == null)
            {
                throw new BusinessException("用户不存在");
            }
            var response = user.Adapt<UserInfoResponse>();

            return response;

        }
    }
}
