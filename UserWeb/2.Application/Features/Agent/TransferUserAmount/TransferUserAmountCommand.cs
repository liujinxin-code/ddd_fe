using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Agent
{
    public record class TransferUserAmountCommand(long ChildrenUserid, decimal transferAmount) : IRequest<ApiResult>;

}
