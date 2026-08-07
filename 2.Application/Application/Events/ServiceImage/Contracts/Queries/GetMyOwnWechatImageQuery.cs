using Application.Common.Models;
using Application.Common.Models.ServiceImage;
using MediatR;

namespace Application.Events.ServiceImage.Contracts.Queries
{
    /// <summary>
    /// 获取当前用户自己上传的客服微信图片（代理后台预览用）。
    /// </summary>
    public record class GetMyOwnWechatImageQuery : IRequest<ApiResult<AgentWechatImageItem>>;
}
