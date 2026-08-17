using Application.Common.Models;
using Application.Features.Notice.Models;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Notice
{
    /// <summary>
    /// 首页公告（分页）：返回置顶公告（notice_type=1）与普通公告（notice_type=2）。
    /// 排序为“置顶优先、同类型内按创建时间倒序”，由仓储固定，Sorting 不参与（业务排序固定）。
    /// 首页默认展示“1 条置顶 + 5 条普通”时，前端可传 PageSize=6 取第一页；分页通用。
    /// 分页参数（PageIndex / PageSize）继承自 PagedQuery；页码/页大小已在请求中，响应无需回显，总条数走 ApiResult.DataTotal。
    /// </summary>
    public record class GetHomepageNoticesQuery() : PagedQuery, IRequest<ApiResult<List<NoticeResponse>>>
    {
    }
}
