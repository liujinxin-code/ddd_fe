using Application.Common.Models;
using Application.Features.Agent.Models;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Agent
{
    public record class UpdateChildrenUserStatusCommand(long ChildrenUserid, TkUserStatus UserStatus) : IRequest<Unit>;
}
