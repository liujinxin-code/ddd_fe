using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Events.Agent.Contracts.Commands
{
    public record class TransferUserAmountCommand(long ChildrenUserid, decimal transferAmount) : IRequest<ApiResult>;

}
