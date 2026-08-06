using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Common.Models.ConsumeLogs;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class ConsumeLogRepository(AppDbContext appDbContext) : IRepository<ConsumeLog>, IConsumeLogRepository
    {
        public Task<bool> AddAsync(ConsumeLog entity, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task AddRangeAsync(IEnumerable<ConsumeLog> logs, CancellationToken ct = default)
        => await appDbContext.ConsumeLogs.AddRangeAsync(logs, ct);

        public Task<ConsumeLog?> GetByIdAsNoTrackingAsync(long id, CancellationToken ct = default)
     => appDbContext.ConsumeLogs.AsNoTracking().Where(t => t.ConsumeId == id).FirstOrDefaultAsync();

        public Task<ConsumeLog?> GetByIdAsync(long id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public void Update(ConsumeLog entity, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 消费流水读模型：按用户过滤，可选按类型/流水号检索，白名单字段排序后分页投影。
        /// tk_consumelog 已是扁平流水表，无需连表。
        /// </summary>
        public async Task<(IReadOnlyList<ConsumeLogListItem> Items, int Total)> GetPagedByUserAsync(
            int userId,
            int consumeStatus,
            string? keyword,
            int pageIndex,
            int pageSize,
            string? sortField,
            bool sortDesc,
            CancellationToken ct = default)
        {
            var logs = appDbContext.ConsumeLogs.AsNoTracking().Where(l => l.UserId == userId);

            // -1 表示不过滤类型；否则按 ConsumeStatus 精确匹配。
            if (consumeStatus >= 0)
            {
                logs = logs.Where(l => (int)l.ConsumeStatus == consumeStatus);
            }

            // 流水号模糊检索，LIKE 由 EF 参数化，无注入风险。
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                logs = logs.Where(l => EF.Functions.Like(l.ConsumeNo, $"%{kw}%"));
            }

            int total = await logs.CountAsync(ct);
            if (total == 0)
            {
                return (new List<ConsumeLogListItem>(), 0);
            }

            logs = ApplyConsumeSorting(logs, sortField, sortDesc);

            var items = await logs
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new ConsumeLogListItem
                {
                    ConsumeId = l.ConsumeId,
                    ConsumeNo = l.ConsumeNo,
                    ConsumeStatus = (int)l.ConsumeStatus,
                    BeforeAmount = l.BeforeAmount,
                    AfterAmount = l.AfterAmount,
                    ChangeAmount = l.AfterAmount - l.BeforeAmount,
                    CreateTime = l.CreateTime,
                })
                .ToListAsync(ct);

            return (items, total);
        }

        /// <summary>
        /// 仅允许白名单内的字段排序，避免任意列名导致 EF 翻译失败或注入。缺省按时间倒序。
        /// </summary>
        private static IQueryable<ConsumeLog> ApplyConsumeSorting(IQueryable<ConsumeLog> query, string? sortField, bool sortDesc)
        {
            switch ((sortField ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "beforeamount":
                    return sortDesc ? query.OrderByDescending(l => l.BeforeAmount) : query.OrderBy(l => l.BeforeAmount);
                case "afteramount":
                    return sortDesc ? query.OrderByDescending(l => l.AfterAmount) : query.OrderBy(l => l.AfterAmount);
                case "consumestatus":
                    return sortDesc ? query.OrderByDescending(l => l.ConsumeStatus) : query.OrderBy(l => l.ConsumeStatus);
                case "createtime":
                    return sortDesc ? query.OrderByDescending(l => l.CreateTime) : query.OrderBy(l => l.CreateTime);
                default:
                    return query.OrderByDescending(l => l.CreateTime);
            }
        }
    }
}
