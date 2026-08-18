using Application.Abstractions;
using Application.Common.Models;
using Application.Abstractions.Repositories;

using Application.Features.Ticket;
using Domain.Entities;
using MediatR;
using Shared.Utilities;
using System.Text.Json;

using Shared.Exceptions;
namespace Application.Features.Ticket;

/// <summary>提交工单。用户身份来自 ICurrentUser（JWT），图片 URL 已在上传阶段落地。</summary>
public class CreateTicketCommandHandler(
    ITicketRepository ticketRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<CreateTicketCommand, long>
{
    public async Task<long> Handle(CreateTicketCommand cmd, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.Userid <= 0)
        {
            throw new UnauthorizedDomainException();
        }

        var imagesJson = cmd.TicketImages == null || cmd.TicketImages.Count == 0
            ? "[]"
            : JsonSerializer.Serialize(cmd.TicketImages);

        var ticketNo = Utils.GenerateSerialNo(serialNoPre: "T");
        var ticket = new TkTicket(currentUser.Userid, ticketNo, cmd.TicketContent.Trim(), cmd.TicketType, imagesJson);

        await ticketRepository.AddAsync(ticket, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return ticket.TicketId;
    }
}
