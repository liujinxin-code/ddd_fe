using Application.Common.Models;
using Application.Common.Models.Notice;
using MediatR;

namespace Application.Events.Notice.Contracts.Queries
{
    /// <summary>
    /// 弹窗公告：返回最新一条弹窗公告（notice_type=3）。弹窗公告全局仅一条，不存在时返回 Data=null（前端据此不弹窗）。
    /// 无请求参数；公告的新增 / 修改由后台判断，前台无需、也不可干预。
    /// </summary>
    public record class GetPopupNoticeQuery() : IRequest<ApiResult<NoticeListItem>>
    {
    }
}
