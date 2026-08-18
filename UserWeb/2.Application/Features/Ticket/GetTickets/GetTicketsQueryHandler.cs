using Application.Abstractions;
using Shared.Exceptions;
using Application.Common.Models;
using Application.Abstractions.Repositories;

using Application.Features.Ticket.Models;
using Application.Features.Ticket;
using MediatR;
using Shared.Utilities;
using System;

namespace Application.Features.Ticket;

/// <summary>「我的工单」分页查询。只读当前登录用户本人提交的工单（用户 id 来自 ICurrentUser，杜绝越权）。</summary>
public class GetTicketsQueryHandler(
    ITicketRepository ticketRepository,
    ICurrentUser currentUser)
    : IRequestHandler<GetTicketsQuery, PagedResult<TicketResponse>>
{
    public async Task<PagedResult<TicketResponse>> Handle(GetTicketsQuery query, CancellationToken ct)
    {
                    if (!currentUser.IsAuthenticated || currentUser.Userid <= 0)
            {
                throw new UnauthorizedDomainException();
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

        return new PagedResult<TicketResponse>(items.ToList(), total);
    }
}
