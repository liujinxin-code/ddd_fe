using Application.Common.Models;
using MediatR;

namespace Application.Features.User
{
    /// <summary>
    /// 生成/刷新当前登录用户的长期 API Key。
    /// 无需密码：调用方已登录（JWT 鉴权），直接生成新的 Key 并覆盖原 Key。
    /// 生成的 Key 是一个 20 年有效期的 JWT，claim 中 client_type=API。
    /// </summary>
    public record GenerateApiKeyCommand : IRequest<ApiResult<GenerateApiKeyResponse>>;

    /// <summary>
    /// 查看当前 API Key（需验证登录密码）。
    /// 不会生成新 Key，仅返回数据库中已存在的 api_key。
    /// </summary>
    public record ViewApiKeyCommand(string Password) : IRequest<ApiResult<GenerateApiKeyResponse>>;

    /// <summary>
    /// API Key 生成/查看结果
    /// </summary>
    public record GenerateApiKeyResponse(string ApiKey);
}
