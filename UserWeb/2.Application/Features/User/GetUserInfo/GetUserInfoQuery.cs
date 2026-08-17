using Application.Common.Models;
using Application.Features.User.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.User
{
    public record class GetUserInfoQuery() : IRequest<ApiResult<UserInfoResponse>>
    {
    }
}
