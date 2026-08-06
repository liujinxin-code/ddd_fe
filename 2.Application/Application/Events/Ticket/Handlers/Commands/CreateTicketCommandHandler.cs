using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Events.Ticket.Contracts.Commands;
using Domain.Entities;
using MediatR;
using Shared.Utilities;
using System.Text.Json;

namespace Application.Events.Ticket.Handlers.Commands;

/// <summary>提交工单。用户身份来自 ICurrentUser（JWT），图片 URL 已在上传阶段落地。</summary>
public class CreateTicketCommandHandler(
    ITicketRepository ticketRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<CreateTicketCommand, ApiResult<long>>
{
    public async Task<ApiResult<long>> Handle(CreateTicketCommand cmd, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.Userid <= 0)
        {
            return new ApiResult<long> { Code = 401, Message = "登录失效", Data = 0, DataTotal = 0 };
        }

        var imagesJson = cmd.TicketImages == null || cmd.TicketImages.Count == 0
            ? "[]"
            : JsonSerializer.Serialize(cmd.TicketImages);

        var ticketNo = Utils.GenerateSerialNo(serialNoPre: "T");
        var ticket = new TkTicket(currentUser.Userid, ticketNo, cmd.TicketContent.Trim(), cmd.TicketType, imagesJson);

        await ticketRepository.AddAsync(ticket, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new ApiResult<long> { Code = 200, Message = "提交成功", Data = ticket.TicketId, DataTotal = 1 };
    }
}
