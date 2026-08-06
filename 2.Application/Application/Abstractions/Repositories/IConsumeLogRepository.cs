using Application.Common.Models.ConsumeLogs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions.Repositories
{
    public interface IConsumeLogRepository : IRepository<ConsumeLog>
    {
        Task AddRangeAsync(IEnumerable<ConsumeLog> logs, CancellationToken ct = default);

        /// <summary>
        /// 分页查询指定用户的消费流水。consumeStatus 传 -1 表示不过滤类型；
        /// keyword 模糊匹配流水号；排序白名单由实现方控制。
        /// </summary>
        Task<(IReadOnlyList<ConsumeLogListItem> Items, int Total)> GetPagedByUserAsync(
            int userId,
            int consumeStatus,
            string? keyword,
            int pageIndex,
            int pageSize,
            string? sortField,
            bool sortDesc,
            CancellationToken ct = default);
    }
}
