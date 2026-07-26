using Application.Common.Models;
using Application.Events.User.Contracts.Commands;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Events.Agent.Contracts.Commands
{
    public record class CreateChildrenCommand(string Username, string Email, string Password) : IRequest<ApiResult>
    {
        public long AgentUserid { get; set; }
    }
}
