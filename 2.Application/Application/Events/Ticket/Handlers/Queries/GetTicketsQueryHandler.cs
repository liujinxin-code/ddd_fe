using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Common.Models.Response.Ticket;
using Application.Events.Ticket.Contracts.Queries;
using MediatR;
using Shared.Utilities;
using System;

namespace Application.Events.Ticket.Handlers.Queries;

/// <summary>「我的工单」分页查询。只读当前登录用户本人提交的工单（用户 id 来自 ICurrentUser，杜绝越权）。</summary>
public class GetTicketsQueryHandler(
    ITicketRepository ticketRepository,
    ICurrentUser currentUser)
    : IRequestHandler<GetTicketsQuery, ApiResult<List<TicketResponse>>>
{
    public async Task<ApiResult<List<TicketResponse>>> Handle(GetTicketsQuery query, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.Userid <= 0)
        {
            return new ApiResult<List<TicketResponse>>
            {
                Code = 401,
                Message = "登录失效",
                Data = new List<TicketResponse>(),
                DataTotal = 0,
            };
        }

        string sortField;
        bool sortDesc;
        if (string.IsNullOrWhiteSpace(query.Sorting))
        {
            sortField = "createtime";
            sortDesc = true;
        }
        else
        {
            var parts = query.Sorting.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            sortField = parts[0];
            sortDesc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
        }

        var (items, total) = await ticketRepository.GetPagedByUserAsync(
            currentUser.Userid,
            query.TicketStatus,
            query.TicketType,
            query.Keyword,
            query.PageIndex,
            query.PageSize,
            sortField,
            sortDesc,
            ct);

        return new ApiResult<List<TicketResponse>>
        {
            Code = 200,
            Message = "Success!",
            Data = items.ToList(),
            DataTotal = total,
        };
    }
}
