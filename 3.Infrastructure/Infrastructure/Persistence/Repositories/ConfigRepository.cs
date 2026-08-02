using Application.Abstractions.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class ConfigRepository(AppDbContext appDbContext) : IConfigRepository
    {
        /// <summary>
        /// 按 平台 + 子平台 过滤，仅取前台可见（config_status=1 全部启用）的配置，分页+排序白名单。
        /// </summary>
        public async Task<(IReadOnlyList<TkConfig> Items, int Total)> GetConfigsAsync(
            int platformId, int subPlatformId, int pageIndex, int pageSize,
            string? sortField, bool sortDesc, string? keyword = null, CancellationToken ct = default)
        {
            var query = appDbContext.TkConfigs.AsNoTracking()
                .Where(c => c.PlatformId == platformId
                         && c.SubPlatformId == subPlatformId
                         && c.ConfigStatus == ConfigStatus.AllEnabled);

            // 按业务名或 config_id 模糊检索（keyword 为空时不加条件；LIKE 模式由 EF 参数化，无注入风险）。
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                if (int.TryParse(kw, out var configIdKeyword))
                {
                    query = query.Where(c => EF.Functions.Like(c.ConfigName, $"%{kw}%") || c.ConfigId == configIdKeyword);
                }
                else
                {
                    query = query.Where(c => EF.Functions.Like(c.ConfigName, $"%{kw}%"));
                }
            }

            int total = await query.CountAsync(ct);

            query = ApplyConfigSorting(query, sortField, sortDesc);

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<Dictionary<int, decimal>> GetUserCustomPricesAsync(long userId, IEnumerable<int> configIds, CancellationToken ct = default)
        {
            var ids = configIds as List<int> ?? configIds.ToList();
            if (ids.Count == 0)
            {
                return new Dictionary<int, decimal>();
            }

            // tk_price_user_custom.userid 在表中为 int，userId 为 long，按设计阶段用户 id 落在 int 范围内，此处显式转换。
            int uid = (int)userId;
            return await appDbContext.TkPriceUserCustoms.AsNoTracking()
                .Where(p => p.UserId == uid && ids.Contains(p.ConfigId))
                .ToDictionaryAsync(p => p.ConfigId, p => p.CustomPrice, ct);
        }

        public async Task<Dictionary<int, decimal>> GetAgentCustomPricesAsync(long agentUserId, IEnumerable<int> configIds, CancellationToken ct = default)
        {
            var ids = configIds as List<int> ?? configIds.ToList();
            if (ids.Count == 0)
            {
                return new Dictionary<int, decimal>();
            }

            int aid = (int)agentUserId;
            return await appDbContext.TkPriceUserCustoms.AsNoTracking()
                .Where(p => p.UserId == aid && ids.Contains(p.ConfigId))
                .ToDictionaryAsync(p => p.ConfigId, p => p.CustomPrice, ct);
        }

        public async Task<Dictionary<int, decimal>> GetAgentMarkupsAsync(long agentUserId, IEnumerable<int> configIds, CancellationToken ct = default)
        {
            var ids = configIds as List<int> ?? configIds.ToList();
            if (ids.Count == 0)
            {
                return new Dictionary<int, decimal>();
            }

            return await appDbContext.TkPriceAgentMarkups.AsNoTracking()
                .Where(p => p.AgentUserId == agentUserId && ids.Contains(p.ConfigId))
                .ToDictionaryAsync(p => p.ConfigId, p => p.MarkupAddPrice, ct);
        }

        public async Task<TkPriceOverall?> GetAgentOverallAsync(long agentUserId, CancellationToken ct = default)
        {
            return await appDbContext.TkPriceOveralls.AsNoTracking()
                .FirstOrDefaultAsync(o => o.UserId == agentUserId, ct);
        }

        /// <summary>
        /// 仅允许白名单内的字段排序，避免任意列名导致 EF 翻译失败或注入。
        /// </summary>
        private static IQueryable<TkConfig> ApplyConfigSorting(IQueryable<TkConfig> query, string? sortField, bool sortDesc)
        {
            var field = (sortField ?? string.Empty).Trim().ToLowerInvariant();
            System.Linq.Expressions.Expression<System.Func<TkConfig, object>> keySelector = field switch
            {
                "configid" => c => c.ConfigId,
                "configname" => c => c.ConfigName,
                "configprice" => c => c.ConfigPrice,
                "minquantity" => c => c.MinQuantity,
                "maxquantity" => c => c.MaxQuantity,
                "createtime" => c => c.CreateTime,
                "configsort" => c => c.ConfigSort,
                _ => c => c.ConfigSort
            };

            return sortDesc ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}
