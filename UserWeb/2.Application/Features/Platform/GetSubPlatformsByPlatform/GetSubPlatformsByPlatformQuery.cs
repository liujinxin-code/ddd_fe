using Application.Common.Models;
using Application.Features.Platform.Models;
using MediatR;

namespace Application.Features.Platform
{
    /// <summary>
    /// 根据 platform_id 获取该平台下的业务类型列表，用于二级联动下拉（sub_platform_id + sub_platform_name）。
    /// </summary>
    public record class GetSubPlatformsByPlatformQuery(int PlatformId = 0) : IRequest<ApiResult<List<SubPlatformResponse>>>
    {
    }
}
