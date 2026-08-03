using Application.Common.Models;
using Application.Common.Models.User;
using Application.Events.User.Contracts.Commands;
using Application.Events.User.Contracts.Queries;
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
        /// 注册
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("register"), AllowAnonymous]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        public async Task<ApiResult> RegisterAsync([FromBody] RegisterCommand cmd, CancellationToken ct) => await mediator.Send(cmd, ct);
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("login"), AllowAnonymous]
        [ProducesResponseType(typeof(ApiResult<LoginResponse>), StatusCodes.Status200OK)]
        public async Task<ApiResult<LoginResponse>> LoginAsync([FromBody] LoginQuery cmd, CancellationToken ct) => await mediator.Send(cmd, ct);
        /// <summary>
        /// 退出登录
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("logout")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        public async Task<ApiResult> LogoutAsync()
        {
            return await mediator.Send(new LogoutCommand());
        }
        /// <summary>
        /// 获取用户个人信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("info")]
        [ProducesResponseType(typeof(ApiResult<GetUserInfoQuery>), StatusCodes.Status200OK)]
        public async Task<ApiResult<UserInfoResponse>> GetUserInfoAsync()
        {
            return await mediator.Send(new GetUserInfoQuery());
        }

        /// <summary>
        /// 修改当前登录用户密码（需验证原密码）。
        /// </summary>
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        public async Task<ApiResult> ChangePasswordAsync([FromBody] ChangePasswordCommand cmd, CancellationToken ct)
        {
            return await mediator.Send(cmd, ct);
        }

    }
}
