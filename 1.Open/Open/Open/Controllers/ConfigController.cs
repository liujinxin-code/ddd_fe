using Application.Common.Models;
using Application.Common.Models.Config;
using Application.Events.Config.Contracts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Open.Controllers
{
    [Authorize]
    public class ConfigController(IMediator mediator) : BaseController
    {
        /// <summary>
        /// 首页业务列表：用户选定平台与业务类型后，分页获取该业务类型下“前台可见”的业务配置，
        /// 并附带“当前登录用户看到的价格”（已按单独定价/代理加价规则计算）。
        /// 请求体：{ "PlatformId": 1, "SubPlatformId": 2, "PageIndex": 1, "PageSize": 20, "Sorting": "configprice desc" }
    /// 注意：当前登录用户由 ICurrentUser 注入，前台无需（也不可伪造）传递。
    /// </summary>
    [HttpPost("list")]
    [ProducesResponseType(typeof(ApiResult<List<ConfigListItem>>), StatusCodes.Status200OK)]
    public async Task<ApiResult<List<ConfigListItem>>> GetConfigsAsync([FromBody] GetConfigsQuery query, CancellationToken ct)
    {
        return await mediator.Send(query, ct);
    }
    }
}
