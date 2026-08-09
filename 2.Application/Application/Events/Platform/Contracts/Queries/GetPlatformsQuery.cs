using Application.Common.Models;
using Application.Common.Models.Response.Platform;
using MediatR;

namespace Application.Events.Platform.Contracts.Queries
{
    /// <summary>
    /// 获取平台列表，用于下拉框展示。无参数，返回全部平台（platform_id + platform_name）。
    /// </summary>
    public record class GetPlatformsQuery() : IRequest<ApiResult<List<PlatformResponse>>>
    {
    }
}
