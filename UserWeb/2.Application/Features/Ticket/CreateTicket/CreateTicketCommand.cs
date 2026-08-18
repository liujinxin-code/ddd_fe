using MediatR;
using Application.Common.Models;
using System.Collections.Generic;

namespace Application.Features.Ticket;

/// <summary>
/// 提交工单。图片 URL 列表已在上传接口取得（/api/ticket/upload），此处仅存储引用。
/// 当前登录用户由 JWT 注入（ICurrentUser），不可伪造。
/// </summary>
public record CreateTicketCommand(
    /// <summary>工单内容，1-3000 字</summary>
    string TicketContent,
    /// <summary>问题类型：0 订单问题 / 1 下单问题 / 2 网站问题 / 3 网站建议</summary>
    int TicketType,
    /// <summary>已上传的图片相对 URL 列表，最多 5 张</summary>
    List<string>? TicketImages = null)
    : IRequest<long>;
