using Application.Abstractions;
using Application.Abstractions.Passwords;
using Application.Common.Models;
using Application.Events.Agent.Contracts;
using Application.Events.Agent.Contracts.Commands;
using Application.Events.User.Contracts;
using Application.Events.User.Contracts.Commands;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Events.Agent
{
    public class CreateChildrenCommandHandler(
        IAgentRepository agentRepository
    ) : IRequestHandler<CreateChildrenCommand, ApiResult>
    {
        public async Task<ApiResult> Handle(CreateChildrenCommand cmd, CancellationToken ct)
        {

            return ApiResult.Successed();
        }
    }
}
