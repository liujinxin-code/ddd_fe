using Application.Common.Models;
using MediatR;

namespace Application.Events.ServiceImage.Contracts.Commands
{
    /// <summary>
    /// 代理上传/更换自己的客服微信图片。
    /// </summary>
    public record class UploadAgentWechatImageCommand(string ImageUrl) : IRequest<ApiResult>;
}
