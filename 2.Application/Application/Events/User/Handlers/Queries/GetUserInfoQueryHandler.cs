using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Common.Models.User;
using Application.Events.User.Contracts.Queries;
using Domain.Entities;
using Mapster;
using MediatR;
using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Events.User.Handlers.Queries
{
    public class GetUserInfoQueryHandler(ITkUserRepository tkUserRepository, ICurrentUser currentUser) : IRequestHandler<GetUserInfoQuery, ApiResult<UserInfoResponse>>
    {
        public async Task<ApiResult<UserInfoResponse>> Handle(GetUserInfoQuery query, CancellationToken ct)
        {
            TkUser? user = await tkUserRepository.GetByIdAsync(currentUser.Userid, ct);
            if (user == null)
            {
                throw new BusinessException("用户不存在");
            }
            var response = user.Adapt<UserInfoResponse>();

            return ApiResult<UserInfoResponse>.Successed(response);

        }
    }
}
