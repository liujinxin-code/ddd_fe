using Application.Abstractions.Repositories;
using Application.Common.Models.Agent;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class AgentPricingRepository(AppDbContext appDbContext) : IAgentPricingRepository
    {
        public async Task<TkPriceOverall?> GetOverallByUserAsync(long userId, CancellationToken ct = default)
        {
            return await appDbContext.TkPriceOveralls
                .FirstOrDefaultAsync(x => x.UserId == userId, ct);
        }

        public async Task<bool> AddOverallAsync(TkPriceOverall entity, CancellationToken ct = default)
        {
            return await appDbContext.TkPriceOveralls.AddAsync(entity, ct) != null;
        }

        public async Task<TkPriceAgentMarkup?> GetMarkupAsync(int configId, long agentUserId, CancellationToken ct = default)
        {
            return await appDbContext.TkPriceAgentMarkups
                .FirstOrDefaultAsync(x => x.ConfigId == configId && x.AgentUserId == agentUserId, ct);
        }

        public async Task<bool> AddMarkupAsync(TkPriceAgentMarkup entity, CancellationToken ct = default)
        {
            return await appDbContext.TkPriceAgentMarkups.AddAsync(entity, ct) != null;
        }

        public void DeleteMarkup(TkPriceAgentMarkup entity, CancellationToken ct = default)
        {
            appDbContext.TkPriceAgentMarkups.Remove(entity);
        }

        public async Task<(IReadOnlyList<AgentMarkupWithConfig> Items, int Total)> GetMarkupsByAgentAsync(
            long agentUserId, int pageIndex, int pageSize, string? keyword, CancellationToken ct = default)
        {
            var query = from m in appDbContext.TkPriceAgentMarkups.AsNoTracking()
                        join c in appDbContext.TkConfigs.AsNoTracking()
                            on m.ConfigId equals c.ConfigId
                        where m.AgentUserId == agentUserId
                        select new AgentMarkupWithConfig { Markup = m, Config = c };

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                if (int.TryParse(kw, out var configIdKeyword))
                {
                    query = query.Where(x => EF.Functions.Like(x.Config.ConfigName, $"%{kw}%") || x.Config.ConfigId == configIdKeyword);
                }
                else
                {
                    query = query.Where(x => EF.Functions.Like(x.Config.ConfigName, $"%{kw}%"));
                }
            }

            int total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.Markup.CreateTime)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<IReadOnlyList<int>> GetMarkupConfigIdsByAgentAsync(long agentUserId, CancellationToken ct = default)
        {
            return await appDbContext.TkPriceAgentMarkups.AsNoTracking()
                .Where(m => m.AgentUserId == agentUserId)
                .Select(m => m.ConfigId)
                .Distinct()
                .ToListAsync(ct);
        }
    }
}
