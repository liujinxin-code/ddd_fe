using Application.Abstractions.Auth;
using Application.Common.Models;
using Application.Events.User.Contracts.Commands;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Events.User.Handles.Commands
{
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
