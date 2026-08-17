using Application.Common.Models;
using Application.Features.User;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Agent
{
    public record class CreateChildrenCommand(string Username, string Email, string Password) : IRequest<ApiResult>;
}
