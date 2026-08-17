using Application.Common.Models;
using Application.Common.Models.Response.ServiceImage;
using Application.Events.ServiceImage.Contracts.Commands;
using Application.Events.ServiceImage.Contracts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Open.Controllers
{
    /// <summary>
    /// 客服微信图片：当前用户查看应展示的图片、代理上传/更换自己的图片。
    /// </summary>
    [Authorize]
    public class ServiceImageController(IMediator mediator) : BaseController
    {
        /// <summary>
        /// 获取当前用户应展示的客服微信图片。
        /// 代理返回系统客服图片；普通用户返回上级代理图片，未上传则返回系统客服图片；无上级返回系统客服图片。
        /// </summary>
        [HttpPost("my-wechat")]
        [ProducesResponseType(typeof(ApiResult<AgentWechatImageResponse>), StatusCodes.Status200OK)]
        public async Task<ApiResult<AgentWechatImageResponse>> GetMyWechatImageAsync(CancellationToken ct)
        {
            return await mediator.Send(new GetMyAgentWechatImageQuery(), ct);
        }

        /// <summary>
        /// 获取当前用户自己上传的客服微信图片（代理后台预览自己的图片）。
        /// </summary>
        [Authorize(Roles = "User.Agent")]
        [HttpPost("my-own")]
        [ProducesResponseType(typeof(ApiResult<AgentWechatImageResponse>), StatusCodes.Status200OK)]
        public async Task<ApiResult<AgentWechatImageResponse>> GetMyOwnWechatImageAsync(CancellationToken ct)
        {
            return await mediator.Send(new GetMyOwnWechatImageQuery(), ct);
        }

        /// <summary>
        /// 代理上传/更换自己的客服微信图片。
        /// </summary>
        [Authorize(Roles = "User.Agent")]
        [HttpPost("upload")]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        public async Task<ApiResult> UploadAgentWechatImageAsync([FromBody] UploadAgentWechatImageCommand cmd, CancellationToken ct)
        {
            return await mediator.Send(cmd, ct);
        }
    }
}
