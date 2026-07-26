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
    public class AgentController(IMediator mediator) : BaseController
    {
        /// <summary>
        /// 代理创建下级用户
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost, AllowAnonymous]
        [ProducesResponseType(typeof(ApiResult<LoginResponse>), StatusCodes.Status200OK)]
        public async Task<ApiResult> CreateChildrenAsync([FromBody] CreateChildrenCommand cmd, CancellationToken ct) => await mediator.Send(cmd, ct);
    }
}
