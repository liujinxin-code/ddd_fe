using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
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
    }
}
