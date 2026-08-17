using Application.Common.Models;
using Application.Features.Agent.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Agent
{
    public record class ResetChildrenPasswordCommand(long ChildrenUserid) : IRequest<ApiResult<ResetChildrenPasswordResponse>>;
}
