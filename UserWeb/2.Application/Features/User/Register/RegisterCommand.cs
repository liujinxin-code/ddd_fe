using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.User
{
    public record RegisterCommand(string username, string email, string password, string agentDomain)
      : IRequest<ApiResult>;   //返回值

}
