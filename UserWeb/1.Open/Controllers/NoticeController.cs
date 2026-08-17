using Application.Common.Models;
using Application.Features.Notice.Models;
using Application.Features.Notice;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Open.Controllers
{
    [Authorize]
    public class NoticeController(IMediator mediator) : BaseController
    {
        /// <summary>
        /// 首页公告（分页）：返回置顶公告 + 普通公告，置顶优先、同类型内按创建时间倒序。
        /// 请求体：{ "PageIndex": 1, "PageSize": 6 }（首页默认“1 条置顶 + 5 条普通”可传 PageSize=6）。
        /// 公告为公共展示内容，无需注入用户；业务排序固定，Sorting 不参与。
        /// </summary>
        [HttpPost("homepage")]
        [ProducesResponseType(typeof(ApiResult<List<NoticeResponse>>), StatusCodes.Status200OK)]
        public async Task<ApiResult<List<NoticeResponse>>> GetHomepageNoticesAsync([FromBody] GetHomepageNoticesQuery query, CancellationToken ct)
        {
            return await mediator.Send(query, ct);
        }

        /// <summary>
        /// 弹窗公告：返回最新一条弹窗公告（notice_type=3）。无请求参数（请求体传 {} 即可）。
        /// 弹窗公告全局仅一条，不存在时返回 Data=null，前端据此不弹窗。
        /// </summary>
        [HttpPost("popup")]
        [ProducesResponseType(typeof(ApiResult<NoticeResponse>), StatusCodes.Status200OK)]
        public async Task<ApiResult<NoticeResponse>> GetPopupNoticeAsync([FromBody] GetPopupNoticeQuery query, CancellationToken ct)
        {
            return await mediator.Send(query, ct);
        }
    }
}
