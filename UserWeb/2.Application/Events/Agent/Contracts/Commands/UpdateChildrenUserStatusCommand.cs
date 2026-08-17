using Application.Common.Models;
using Application.Common.Models.Response.Agent;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Events.Agent.Contracts.Commands
{
    public record class UpdateChildrenUserStatusCommand(long ChildrenUserid, TkUserStatus UserStatus) : IRequest<ApiResult>;
}
