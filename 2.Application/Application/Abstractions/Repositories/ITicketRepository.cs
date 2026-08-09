using Application.Common.Models.Response.Ticket;
using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface ITicketRepository : IRepository<TkTicket>
{
    /// <summary>
    /// 按用户分页查询工单。images 字段由 JSON 反序列化为字符串列表。
    /// 状态/类型为 -1 时不过滤；关键词模糊匹配工单内容。排序缺省按创建时间倒序。
    /// </summary>
    Task<(IReadOnlyList<TicketResponse> Items, int Total)> GetPagedByUserAsync(
        int userId,
        int ticketStatus,
        int ticketType,
        string? keyword,
        int pageIndex,
        int pageSize,
        string? sortField,
        bool sortDesc,
        CancellationToken ct = default);
}
