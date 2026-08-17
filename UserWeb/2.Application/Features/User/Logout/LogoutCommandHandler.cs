using Application.Abstractions.Auth;
using Application.Common.Models;
using Application.Features.User;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.User
{
    public class LogoutCommandHandler(ITokenCacheService tokenCacheService,
        ICurrentUser currentUser
   ) : IRequestHandler<LogoutCommand, ApiResult>
    {

        public async Task<ApiResult> Handle(LogoutCommand args, CancellationToken ct)
        {
            await tokenCacheService.RemoveTokenAsync(currentUser.Jti, currentUser.Userid);
            return ApiResult.Successed("退出成功");
        }
    }
}
