using Application.Common.Models;
using Application.Common.Models.Agent;
using Application.Common.Models.User;
using Application.Events.Agent.Contracts.Commands;
using Application.Events.Agent.Contracts.Queries;
using Application.Events.User.Contracts.Queries;
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
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("create-children")]
        [ProducesResponseType(typeof(ApiResult<LoginResponse>), StatusCodes.Status200OK)]
        public async Task<ApiResult> CreateChildrenAsync([FromBody] CreateChildrenCommand cmd, CancellationToken ct)
        {
            cmd = cmd with
            {
                AgentUserid = CurrentUser.Userid
            };
            return await mediator.Send(cmd, ct);
        }
        /// <summary>
        /// 代理向下级用户转赠余额
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("transfer")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        public async Task<ApiResult> TransferUserAmountAsync([FromBody] TransferUserAmountCommand cmd, CancellationToken ct)
        {
            cmd = cmd with
            {
                AgentUserid = CurrentUser.Userid
            };
            return await mediator.Send(cmd, ct);
        }

        /// <summary>
        /// 重置下级用户密码
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ApiResult<ResetChildrenPasswordResponse>), StatusCodes.Status200OK)]
        public async Task<ApiResult<ResetChildrenPasswordResponse>> ResetChildrenPasswordAsync([FromBody] ResetChildrenPasswordCommand cmd, CancellationToken ct)
        {
            cmd = cmd with
            {
                AgentUserid = CurrentUser.Userid
            };
            return await mediator.Send(cmd, ct);
        }

        /// <summary>
        /// 修改下级用户状态
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("update-status")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        public async Task<ApiResult> UpdateChildrenUserStatusAsync([FromBody] UpdateChildrenUserStatusCommand cmd, CancellationToken ct)
        {
            cmd = cmd with
            {
                AgentUserid = CurrentUser.Userid
            };
            return await mediator.Send(cmd, ct);
        }

        /// <summary>
        /// 代理分页查询自己的下级用户（POST 形式），支持按用户名或邮箱关键词模糊匹配（Keyword），以及排序字段（Sorting 形如 "useramount desc"）。
        /// 请求体：{ "PageIndex":1, "PageSize":20, "Keyword":"tom", "Sorting":"useramount desc" }
        /// </summary>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("children")]
        [ProducesResponseType(typeof(ApiResult<List<ChildrenUserListItem>>), StatusCodes.Status200OK)]
        public async Task<ApiResult<List<ChildrenUserListItem>>> GetChildrenUsersAsync([FromBody] GetChildrenUsersQuery query, CancellationToken ct)
        {
            query = query with
            {
                AgentUserid = CurrentUser.Userid
            };
            return await mediator.Send(query, ct);
        }

        /// <summary>
        /// 代理新增 / 修改自己的总体加价百分比（每代理仅一条，首次为新增，之后为修改）。
        /// </summary>
        [HttpPost("overall-price")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        public async Task<ApiResult> UpsertOverallPriceAsync([FromBody] UpsertAgentOverallPriceCommand cmd, CancellationToken ct)
        {
            cmd = cmd with
            {
                UserId = CurrentUser.Userid
            };
            return await mediator.Send(cmd, ct);
        }

        /// <summary>
        /// 代理新增 / 修改自己名下某 config 的加价金额（同一 config 存在则修改，否则新增）。
        /// </summary>
        [HttpPost("markup")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        public async Task<ApiResult> UpsertMarkupAsync([FromBody] UpsertAgentMarkupCommand cmd, CancellationToken ct)
        {
            cmd = cmd with
            {
                AgentUserId = CurrentUser.Userid
            };
            return await mediator.Send(cmd, ct);
        }

        /// <summary>
        /// 代理删除自己名下某 config 的加价记录。
        /// </summary>
        [HttpPost("markup-delete")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        public async Task<ApiResult> DeleteMarkupAsync([FromBody] DeleteAgentMarkupCommand cmd, CancellationToken ct)
        {
            cmd = cmd with
            {
                AgentUserId = CurrentUser.Userid
            };
            return await mediator.Send(cmd, ct);
        }
    }
}
