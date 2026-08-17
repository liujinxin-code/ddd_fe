using Application.Abstractions.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class NoticeRepository(AppDbContext appDbContext) : INoticeRepository
    {
        /// <summary>
        /// 首页公告（置顶 + 普通）：置顶优先，同类型内创建时间倒序；分页。
        /// </summary>
        public async Task<(IReadOnlyList<TkNotice> Items, int Total)> GetHomepageNoticesAsync(
            int pageIndex, int pageSize, CancellationToken ct = default)
        {
            var query = appDbContext.TkNotices.AsNoTracking()
                .Where(n => n.NoticeType == NoticeType.Top || n.NoticeType == NoticeType.Normal);

            int total = await query.CountAsync(ct);

            var items = await query
                .OrderBy(n => n.NoticeType)            // 置顶(1) 排在 普通(2) 之前
                .ThenByDescending(n => n.CreateTime)   // 同类型内最新在前
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        /// <summary>
        /// 弹窗公告：取最新一条（notice_type=3）。
        /// </summary>
        public async Task<TkNotice?> GetPopupNoticeAsync(CancellationToken ct = default)
        {
            return await appDbContext.TkNotices.AsNoTracking()
                .Where(n => n.NoticeType == NoticeType.Popup)
                .OrderByDescending(n => n.CreateTime)
                .FirstOrDefaultAsync(ct);
        }
    }
}
