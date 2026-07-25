using Application.Common.Models;
using Application.Common.Models.User;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.User.Contracts.Queries
{
    public record LoginQuery(string name, string password) : IRequest<ApiResult<LoginResponse>>;
}
