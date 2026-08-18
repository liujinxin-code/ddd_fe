using Open.Common.Models;
using Application.Features.Agent.Models;
using Application.Features.User.Models;
using Application.Features.Agent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Open.Controllers
{
    [Authorize(Roles = "User.Agent")]
    public class AgentController(IMediator mediator) : BaseController
    {
        /// <summary>
        /// 代理创建下级用户
        /// </summary>
        [HttpPost("create-children")]
        [ProducesResponseType(typeof(ApiResult<LoginResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateChildrenAsync([FromBody] CreateChildrenCommand cmd, CancellationToken ct)
         {

            var r = await mediator.Send(cmd, ct);

            return Api(r);

         }

        /// <summary>
        /// 代理向下级用户转赠余额
        /// </summary>
        [HttpPost("transfer")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> TransferUserAmountAsync([FromBody] TransferUserAmountCommand cmd, CancellationToken ct)
         {

            var r = await mediator.Send(cmd, ct);

            return Api(r);

         }

        /// <summary>
        /// 代理将代理收益余额提取到个人用户余额。
        /// </summary>
        [HttpPost("withdraw")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> WithdrawAgentAmountAsync([FromBody] WithdrawAgentAmountCommand cmd, CancellationToken ct)
         {

            var r = await mediator.Send(cmd, ct);

            return Api(r);

         }

        /// <summary>
        /// 重置下级用户密码
        /// </summary>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ApiResult<ResetChildrenPasswordResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ResetChildrenPasswordAsync([FromBody] ResetChildrenPasswordCommand cmd, CancellationToken ct)
         {

            var r = await mediator.Send(cmd, ct);

            return Api(r);

         }

        /// <summary>
        /// 修改下级用户状态
        /// </summary>
        [HttpPost("update-status")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateChildrenUserStatusAsync([FromBody] UpdateChildrenUserStatusCommand cmd, CancellationToken ct)
         {

            var r = await mediator.Send(cmd, ct);

            return Api(r);

         }

        /// <summary>
        /// 代理管理页仪表盘：用户余额、代理余额、下级用户启用数/总数。
        /// </summary>
        [HttpPost("dashboard")]
        [ProducesResponseType(typeof(ApiResult<AgentDashboardResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboardAsync(CancellationToken ct)
         {

            var r = await mediator.Send(new GetAgentDashboardQuery(), ct);

            return Api(r);

         }

        /// <summary>
        /// 代理分页查询自己的下级用户（POST 形式），支持按用户名或邮箱关键词模糊匹配（Keyword），以及排序字段（Sorting 形如 "useramount desc"）。
        /// 请求体：{ "PageIndex":1, "PageSize":20, "Keyword":"tom", "Sorting":"useramount desc" }
        /// </summary>
        [HttpPost("children")]
        [ProducesResponseType(typeof(ApiResult<List<ChildrenUserResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetChildrenUsersAsync([FromBody] GetChildrenUsersQuery query, CancellationToken ct)
         {

            var r = await mediator.Send(query, ct);

            return ApiPaged(r);

         }

        /// <summary>
        /// 代理新增 / 修改自己的总体加价百分比（每代理仅一条，首次为新增，之后为修改）。
        /// </summary>
        [HttpPost("overall-price")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpsertOverallPriceAsync([FromBody] UpsertAgentOverallPriceCommand cmd, CancellationToken ct)
         {

            var r = await mediator.Send(cmd, ct);

            return Api(r);

         }

        /// <summary>
        /// 获取代理当前总体加价百分比。未设置时返回 0。
        /// </summary>
        [HttpPost("overall-price-info")]
        [ProducesResponseType(typeof(ApiResult<AgentOverallPriceResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOverallPriceAsync(CancellationToken ct)
         {

            var r = await mediator.Send(new GetAgentOverallPriceQuery(), ct);

            return Api(r);

         }

        /// <summary>
        /// 代理新增 / 修改自己名下某 config 的加价金额（同一 config 存在则修改，否则新增）。
        /// </summary>
        [HttpPost("markup")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpsertMarkupAsync([FromBody] UpsertAgentMarkupCommand cmd, CancellationToken ct)
         {

            var r = await mediator.Send(cmd, ct);

            return Api(r);

         }

        /// <summary>
        /// 代理删除自己名下某 config 的加价记录。
        /// </summary>
        [HttpPost("markup-delete")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteMarkupAsync([FromBody] DeleteAgentMarkupCommand cmd, CancellationToken ct)
         {

            var r = await mediator.Send(cmd, ct);

            return Api(r);

         }

        /// <summary>
        /// 代理分页获取自己名下的单业务加价列表，支持按业务名称关键字检索。
        /// 请求体：{ "PageIndex":1, "PageSize":6, "Keyword":"粉丝" }
        /// </summary>
        [HttpPost("markups")]
        [ProducesResponseType(typeof(ApiResult<List<AgentMarkupResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMarkupsAsync([FromBody] GetAgentMarkupsQuery query, CancellationToken ct)
         {

            var r = await mediator.Send(query, ct);

            return ApiPaged(r);

         }

        /// <summary>
        /// 代理在「新增单业务加价」模态框中，按平台/业务类型获取尚未加价的 config 列表（含代理基准价）。
        /// 请求体：{ "PlatformId":1, "SubPlatformId":2, "PageIndex":1, "PageSize":100 }
        /// </summary>
        [HttpPost("markup-configs")]
        [ProducesResponseType(typeof(ApiResult<List<AgentMarkupConfigResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMarkupConfigsAsync([FromBody] GetAgentMarkupConfigsQuery query, CancellationToken ct)
         {

            var r = await mediator.Send(query, ct);

            return ApiPaged(r);

         }
    }
}
