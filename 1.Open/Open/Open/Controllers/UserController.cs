using Application.Common.Models;
using Application.Common.Models;
using Application.Common.Models.User;
using Application.User.Contracts.Commands;
using Application.User.Contracts.Queries;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Open.Controllers
{
    [Authorize]
    public class UserController(IMediator mediator) : BaseController
    {
        /// <summary>
        /// ×¢²á
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost, AllowAnonymous]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        public async Task<ApiResult> RegisterAsync([FromBody] RegisterCommand cmd, CancellationToken ct) => await mediator.Send(cmd, ct);
        /// <summary>
        /// µÇÂ¼
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>

        [HttpPost, AllowAnonymous]
        [ProducesResponseType(typeof(ApiResult<LoginResponse>), StatusCodes.Status200OK)]
        public async Task<ApiResult<LoginResponse>> LoginAsync([FromBody] LoginQuery cmd, CancellationToken ct) => await mediator.Send(cmd, ct);
        /// <summary>
        /// ÍË³öµÇÂ¼
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        public async Task<ApiResult> LogoutAsync()
        {
            var newArgs = CurrentUser.Adapt<LogoutCommand>();
            return await mediator.Send(newArgs);
        }
    }
}
