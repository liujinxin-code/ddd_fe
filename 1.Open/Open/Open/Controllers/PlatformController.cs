using Application.Common.Models;
using Application.Common.Models.Platform;
using Application.Events.Platform.Contracts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Open.Controllers
{
    [Authorize]
    public class PlatformController(IMediator mediator) : BaseController
    {
        /// <summary>
        /// 获取平台列表，用于第一个下拉框展示（platform_id + platform_name）。
        /// 请求体：{} 
        /// </summary>
        [HttpPost("list")]
        [ProducesResponseType(typeof(ApiResult<List<PlatformListItem>>), StatusCodes.Status200OK)]
        public async Task<ApiResult<List<PlatformListItem>>> GetPlatformsAsync([FromBody] GetPlatformsQuery query, CancellationToken ct)
        {
            return await mediator.Send(query, ct);
        }

        /// <summary>
        /// 根据 platform_id 获取该平台下的业务类型列表，用于第二个下拉框（二级联动）。
        /// 返回 sub_platform_id + sub_platform_name。
        /// 请求体：{ "PlatformId": 1 }
        /// </summary>
        [HttpPost("subs")]
        [ProducesResponseType(typeof(ApiResult<List<SubPlatformListItem>>), StatusCodes.Status200OK)]
        public async Task<ApiResult<List<SubPlatformListItem>>> GetSubPlatformsByPlatformAsync([FromBody] GetSubPlatformsByPlatformQuery query, CancellationToken ct)
        {
            return await mediator.Send(query, ct);
        }
    }
}
