using Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Abstractions.Repositories
{
    /// <summary>
    /// 客服微信图片仓储。
    /// agent_userid=0 表示系统客服图片；非 0 表示对应代理的客服图片。
    /// </summary>
    public interface IServiceImageRepository : IRepository<TkServiceImage>
    {
        /// <summary>按代理用户 id 获取图片（被追踪）。</summary>
        Task<TkServiceImage?> GetByAgentUserIdAsync(long agentUserId, CancellationToken ct = default);

        /// <summary>
        /// 按代理用户 id 原子 upsert 客服图片：首次插入、后续仅更新图片 URL（CreateTime 保持不变）。
        /// 使用 MySQL <c>ON DUPLICATE KEY UPDATE</c>，避免「先查后插」在并发首次上传时触发唯一键冲突（1062）。
        /// </summary>
        Task UpsertByAgentUserAsync(long agentUserId, string imageUrl, CancellationToken ct = default);
    }
}
