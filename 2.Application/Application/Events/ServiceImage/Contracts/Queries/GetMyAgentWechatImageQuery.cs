using Application.Common.Models;
using Application.Common.Models.ServiceImage;
using MediatR;

namespace Application.Events.ServiceImage.Contracts.Queries
{
    /// <summary>
    /// 获取当前用户应展示的客服微信图片。
    /// 规则：代理看自己系统客服；普通用户看上级代理的图片，上级未上传则看系统客服，无上级也看系统客服。
    /// </summary>
    public record class GetMyAgentWechatImageQuery : IRequest<ApiResult<AgentWechatImageItem>>;
}
