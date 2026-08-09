using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Common.Models.Response.Ticket;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Infrastructure.Persistence.Repositories;

public class TicketRepository(AppDbContext appDbContext) : IRepository<TkTicket>, ITicketRepository
{
    public async Task<bool> AddAsync(TkTicket entity, CancellationToken ct = default)
    {
        await appDbContext.TkTickets.AddAsync(entity, ct);
        return true;
    }

    public Task<TkTicket?> GetByIdAsync(long id, CancellationToken ct = default)
        => appDbContext.TkTickets.FirstOrDefaultAsync(t => t.TicketId == id, ct);

    public Task<TkTicket?> GetByIdAsNoTrackingAsync(long id, CancellationToken ct = default)
        => appDbContext.TkTickets.AsNoTracking().FirstOrDefaultAsync(t => t.TicketId == id, ct);

    public void Update(TkTicket entity, CancellationToken ct = default)
        => appDbContext.TkTickets.Update(entity);

    /// <summary>
    /// 工单读模型：按用户过滤，可选按状态/类型/内容检索，白名单字段排序后分页投影。
    /// ticket_images 为 JSON 数组字符串，投影时反序列化为 List&lt;string&gt;。
    /// </summary>
    public async Task<(IReadOnlyList<TicketResponse> Items, int Total)> GetPagedByUserAsync(
        int userId,
        int ticketStatus,
        int ticketType,
        string? keyword,
        int pageIndex,
        int pageSize,
        string? sortField,
        bool sortDesc,
        CancellationToken ct = default)
    {
        var query = appDbContext.TkTickets.AsNoTracking().Where(t => t.Userid == userId);

        if (ticketStatus >= 0)
            query = query.Where(t => t.TicketStatus == ticketStatus);

        if (ticketType >= 0)
            query = query.Where(t => t.TicketType == ticketType);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            // 同时按工单编号与内容模糊匹配
            query = query.Where(t =>
                EF.Functions.Like(t.TicketNo, $"%{kw}%") ||
                EF.Functions.Like(t.TicketContent, $"%{kw}%"));
        }

        int total = await query.CountAsync(ct);
        if (total == 0)
            return (new List<TicketResponse>(), 0);

        query = ApplyTicketSorting(query, sortField, sortDesc);

        // 先按 EF 可翻译字段投影为匿名类型，JSON 反序列化在内存中完成（避免进入表达式树）。
        var raw = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.TicketId,
                t.TicketNo,
                t.TicketContent,
                t.TicketImages,
                t.TicketResult,
                t.TicketStatus,
                t.TicketType,
                t.Userid,
                t.CreateTime,
            })
            .ToListAsync(ct);

        var items = raw.Select(t => new TicketResponse
        {
            TicketId = t.TicketId,
            TicketNo = t.TicketNo ?? string.Empty,
            TicketContent = t.TicketContent,
            TicketImages = JsonSerializer.Deserialize<List<string>>(t.TicketImages) ?? new List<string>(),
            TicketResult = t.TicketResult,
            TicketStatus = t.TicketStatus,
            TicketType = t.TicketType,
            Userid = t.Userid,
            CreateTime = t.CreateTime,
        }).ToList();

        return (items, total);
    }

    /// <summary>仅允许白名单内的字段排序，避免任意列名导致 EF 翻译失败。缺省按时间倒序。</summary>
    private static IQueryable<TkTicket> ApplyTicketSorting(IQueryable<TkTicket> query, string? sortField, bool sortDesc)
    {
        switch ((sortField ?? string.Empty).Trim().ToLowerInvariant())
        {
            case "ticketstatus":
                return sortDesc ? query.OrderByDescending(t => t.TicketStatus) : query.OrderBy(t => t.TicketStatus);
            case "tickettype":
                return sortDesc ? query.OrderByDescending(t => t.TicketType) : query.OrderBy(t => t.TicketType);
            case "createtime":
            default:
                return sortDesc ? query.OrderByDescending(t => t.CreateTime) : query.OrderBy(t => t.CreateTime);
        }
    }
}
