using Application.Common.Models;
using Application.Common.Models.Response.User;
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

        /// <summary>
        /// 生成/刷新当前用户的长期 API Key。
        /// 直接生成新的 20 年期 JWT（claim 中 client_type=API）并覆盖 tk_user.api_key，旧 Key 立即失效。
        /// </summary>
        [HttpPost("api-key")]
        [ProducesResponseType(typeof(ApiResult<GenerateApiKeyResponse>), StatusCodes.Status200OK)]
        public async Task<ApiResult<GenerateApiKeyResponse>> GenerateApiKeyAsync(CancellationToken ct)
        {
            return await mediator.Send(new GenerateApiKeyCommand(), ct);
        }

        /// <summary>
        /// 查看当前用户的 API Key（需验证登录密码）。
        /// 不会生成新 Key，仅返回数据库中已存在的 api_key；如未生成过则返回空字符串。
        /// </summary>
        [HttpPost("api-key/view")]
        [ProducesResponseType(typeof(ApiResult<GenerateApiKeyResponse>), StatusCodes.Status200OK)]
        public async Task<ApiResult<GenerateApiKeyResponse>> ViewApiKeyAsync([FromBody] ViewApiKeyCommand cmd, CancellationToken ct)
        {
            return await mediator.Send(cmd, ct);
        }

        /// <summary>
        /// 查询当前用户余额。支持 JWT（浏览器端）或 API Key（Authorization: Bearer {apiKey}）两种鉴权方式。
        /// </summary>
        [HttpGet("balance")]
        [ProducesResponseType(typeof(ApiResult<UserBalanceResponse>), StatusCodes.Status200OK)]
        public async Task<ApiResult<UserBalanceResponse>> GetBalanceAsync(CancellationToken ct)
        {
            return await mediator.Send(new GetUserBalanceQuery(), ct);
        }

    }
}
