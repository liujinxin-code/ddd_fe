using Application.Common.Models;
using MediatR;

namespace Application.Features.User
{
    /// <summary>
    /// 查询当前用户余额（用户 id 由 ICurrentUser 注入，支持 JWT 或 API Key 鉴权）。
    /// </summary>
    public record GetUserBalanceQuery : IRequest<ApiResult<UserBalanceResponse>>;

    /// <summary>
    /// 用户余额返回体（API 文档专用：仅返回可用于下单的用户余额，不暴露代理收益）。
    /// </summary>
    public record UserBalanceResponse(decimal UserAmount);
}
