using Application.Common.Models;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Abstractions.Repositories
{
    /// <summary>
    /// 公告只读仓储（前台展示用）。公告的新增 / 修改由后台管理，前台不提供任何写接口。
    /// </summary>
    public interface INoticeRepository
    {
        /// <summary>
        /// 首页公告：返回置顶公告（notice_type=1）与普通公告（notice_type=2）。
        /// 排序规则：置顶优先（type 升序），同类型内按创建时间倒序（最新在前）。分页返回并附带总数。
        /// </summary>
        Task<(IReadOnlyList<TkNotice> Items, int Total)> GetHomepageNoticesAsync(
            int pageIndex, int pageSize, CancellationToken ct = default);

        /// <summary>
        /// 弹窗公告：返回最新一条弹窗公告（notice_type=3，按创建时间倒序取第一条）；不存在返回 null。
        /// </summary>
        Task<TkNotice?> GetPopupNoticeAsync(CancellationToken ct = default);
    }
}
