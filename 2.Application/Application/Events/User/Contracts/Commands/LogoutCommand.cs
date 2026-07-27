using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Events.User.Contracts.Commands
{
    public record LogoutCommand(long Userid, string Username, string Jti) : IRequest<ApiResult>;
}
