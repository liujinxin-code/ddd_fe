using Application.Features.Platform.Models;
using MediatR;

namespace Application.Features.Platform
{
    /// <summary>
    /// 获取平台列表，用于下拉框展示。无参数，返回全部平台（platform_id + platform_name）。
    /// </summary>
    public record class GetPlatformsQuery() : IRequest<List<PlatformResponse>>
    {
    }
}
