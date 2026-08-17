using Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Abstractions.Repositories
{
    /// <summary>
    /// 平台与业务类型只读仓储（前台下拉联动使用，写操作在后台完成，前台不提供）。
    /// </summary>
    public interface IPlatformRepository
    {
        /// <summary>
        /// 获取全部平台列表（platform_id + platform_name）。
        /// </summary>
        Task<IReadOnlyList<TkPlatform>> GetPlatformsAsync(CancellationToken ct = default);

        /// <summary>
        /// 根据 platform_id 获取该平台下的业务类型列表（sub_platform_id + sub_platform_name）。
        /// </summary>
        Task<IReadOnlyList<TkPlatformSub>> GetSubsByPlatformAsync(int platformId, CancellationToken ct = default);
    }
}
