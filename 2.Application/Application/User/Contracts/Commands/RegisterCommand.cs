using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.User.Contracts.Commands
{
    public record RegisterCommand(string username, string email, string password, string agentDomain)
      : IRequest<ApiResult>;   //返回值


    public record LogoutCommand(long Userid, string Username, string Jti) : IRequest<ApiResult>;
}
