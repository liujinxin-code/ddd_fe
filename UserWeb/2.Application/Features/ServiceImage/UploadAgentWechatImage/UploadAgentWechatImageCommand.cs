using Application.Common.Models;
using MediatR;

namespace Application.Features.ServiceImage
{
    /// <summary>
    /// 代理上传/更换自己的客服微信图片。
    /// </summary>
    public record class UploadAgentWechatImageCommand(string ImageUrl) : IRequest<Unit>;
}
