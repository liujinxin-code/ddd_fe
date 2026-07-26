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
    public class UserController(IMediator mediator, ILogger<UserController> _logger) : BaseController
    {
        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost, AllowAnonymous]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        public async Task<ApiResult> RegisterAsync([FromBody] RegisterCommand cmd, CancellationToken ct) => await mediator.Send(cmd, ct);
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>

        [HttpPost, AllowAnonymous]
        [ProducesResponseType(typeof(ApiResult<LoginResponse>), StatusCodes.Status200OK)]
        public async Task<ApiResult<LoginResponse>> LoginAsync([FromBody] LoginQuery cmd, CancellationToken ct) => await mediator.Send(cmd, ct);
        /// <summary>
        /// 退出登录
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
        [HttpGet]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        public async Task<ApiResult<UserInfoResponse>> GetUserInfoAsync()
        {
            var newArgs = CurrentUser.Adapt<GetUserInfoQuery>();

            var userinfo = await mediator.Send(newArgs);
            _logger.LogWarning("GetUserInfoAsync  用户信息：{@0}", userinfo.Data);
            return userinfo;
        }

    }
}
