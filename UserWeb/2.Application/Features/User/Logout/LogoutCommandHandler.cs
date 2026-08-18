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
   ) : IRequestHandler<LogoutCommand, Unit>
    {

        public async Task<Unit> Handle(LogoutCommand args, CancellationToken ct)
        {
            await tokenCacheService.RemoveTokenAsync(currentUser.Jti, currentUser.Userid);
            return Unit.Value;
        }
    }
}
