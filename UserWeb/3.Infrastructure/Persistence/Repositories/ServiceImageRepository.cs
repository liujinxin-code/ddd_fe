using Application.Abstractions;
using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class ServiceImageRepository(AppDbContext appDbContext) : IRepository<TkServiceImage>, IServiceImageRepository
    {
        public async Task<bool> AddAsync(TkServiceImage entity, CancellationToken ct = default)
        {
            return await appDbContext.TkServiceImages.AddAsync(entity, ct) != null;
        }

        public async Task<TkServiceImage?> GetByIdAsync(long id, CancellationToken ct = default)
        {
            return await appDbContext.TkServiceImages.FirstOrDefaultAsync(x => x.ImageId == id, ct);
        }

        public async Task<TkServiceImage?> GetByIdAsNoTrackingAsync(long id, CancellationToken ct = default)
        {
            return await appDbContext.TkServiceImages.AsNoTracking().FirstOrDefaultAsync(x => x.ImageId == id, ct);
        }

        public async Task<TkServiceImage?> GetByAgentUserIdAsync(long agentUserId, CancellationToken ct = default)
        {
            return await appDbContext.TkServiceImages
                .FirstOrDefaultAsync(x => x.AgentUserid == agentUserId, ct);
        }

        public void Update(TkServiceImage entity, CancellationToken ct = default)
        {
            appDbContext.TkServiceImages.Update(entity);
        }

        public async Task UpsertByAgentUserAsync(long agentUserId, string imageUrl, CancellationToken ct = default)
        {
            // 参数化插值：imageUrl / agentUserId 自动成为 SQL 参数，杜绝注入。
            // 并发首次上传时，两个事务都会执行 INSERT，但唯一索引 ux_agent_userid 保证只有一个成功插入，
            // 另一个命中 ON DUPLICATE KEY UPDATE 仅更新 image_url，绝不会抛 1062。
            await appDbContext.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO tk_service_image (image_url, agent_userid, create_time) VALUES ({imageUrl}, {agentUserId}, UTC_TIMESTAMP()) ON DUPLICATE KEY UPDATE image_url = {imageUrl};",
                ct);
        }
    }
}
