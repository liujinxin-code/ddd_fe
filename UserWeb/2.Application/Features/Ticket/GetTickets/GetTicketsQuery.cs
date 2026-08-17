using Application.Common.Models;
using Application.Features.Ticket.Models;
using MediatR;

namespace Application.Features.Ticket;

/// <summary>
/// 工单分页查询。仅返回当前登录用户自己的工单（用户 id 由 ICurrentUser 注入）。
/// </summary>
public record GetTicketsQuery(
    /// <summary>状态筛选：-1 不筛选 / 0 待处理 / 1 已处理</summary>
    int TicketStatus = -1,
    /// <summary>类型筛选：-1 不筛选 / 0 订单问题 / 1 下单问题 / 2 网站问题 / 3 网站建议</summary>
    int TicketType = -1,
    /// <summary>关键词，模糊匹配工单内容</summary>
    string? Keyword = null)
    : PagedQuery, IRequest<ApiResult<List<TicketResponse>>>;
