using Application.Common.Models;
using Application.Common.Models.Agent;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Events.Agent.Contracts.Commands
{
    public record class ResetChildrenPasswordCommand(long ChildrenUserid) : IRequest<ApiResult<ResetChildrenPasswordResponse>>;
}
