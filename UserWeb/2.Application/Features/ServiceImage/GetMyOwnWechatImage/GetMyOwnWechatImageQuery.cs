using Application.Common.Models;
using Application.Features.ServiceImage.Models;
using MediatR;

namespace Application.Features.ServiceImage
{
    /// <summary>
    /// 获取当前用户自己上传的客服微信图片（代理后台预览用）。
    /// </summary>
    public record class GetMyOwnWechatImageQuery : IRequest<ApiResult<AgentWechatImageResponse>>;
}
