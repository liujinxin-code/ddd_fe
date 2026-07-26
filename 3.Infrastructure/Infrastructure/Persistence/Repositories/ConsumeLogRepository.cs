using Application.Abstractions;
using Application.Abstractions.Repositories;
using Application.Events.User.Contracts;
using Domain.Entities;
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

        public Task<ConsumeLog?> GetByIdAsync(long id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public void Update(ConsumeLog entity, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
