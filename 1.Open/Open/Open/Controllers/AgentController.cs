using Application.Common.Models;
using Application.Common.Models.User;
using Application.Events.Agent.Contracts.Commands;
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
        [HttpPost]
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
        [HttpPost]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        public async Task<ApiResult> TransferUserAmountAsync([FromBody] TransferUserAmountCommand cmd, CancellationToken ct)
        {
            cmd = cmd with
            {
                AgentUserid = CurrentUser.Userid
            };
            return await mediator.Send(cmd, ct);
        }

    }
}
