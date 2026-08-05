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
    public class PlatformRepository(AppDbContext appDbContext) : IPlatformRepository
    {
        /// <summary>
        /// 返回已开启（PlatformState=Opened）的平台，供前台下拉使用（只读）。
        /// </summary>
        public async Task<IReadOnlyList<TkPlatform>> GetPlatformsAsync(CancellationToken ct = default)
        {
            return await appDbContext.TkPlatforms.AsNoTracking()
                .Where(p => p.PlatformStatus == PlatformStatus.Opened)
                .ToListAsync(ct);
        }

        /// <summary>
        /// 按 platform_id 过滤该平台下的业务类型（只读）。
        /// </summary>
        public async Task<IReadOnlyList<TkPlatformSub>> GetSubsByPlatformAsync(int platformId, CancellationToken ct = default)
        {
            return await appDbContext.TkPlatformSubs.AsNoTracking()
                .Where(s => s.PlatformId == platformId && s.SubPlatformStatus == SubPlatformStatus.Enabled)
                .ToListAsync(ct);
        }
    }
}
