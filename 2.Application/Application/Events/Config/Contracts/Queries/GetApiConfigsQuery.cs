using Application.Common.Models;
using Application.Common.Models.Response.Config;
using MediatR;

namespace Application.Events.Config.Contracts.Queries
{
    /// <summary>
    /// API 文档：获取当前用户可用的业务配置精简列表（仅含下单所需核心字段）。
    /// 排序字段 Sorting 形如 "configprice desc" / "configsort asc"，缺省按 config_sort 升序。
    /// 分页/排序参数继承自 PagedQuery。
    /// </summary>
    public record class GetApiConfigsQuery(
        int PlatformId = 0,
        int SubPlatformId = 0,
        string Keyword = ""
    ) : PagedQuery, IRequest<ApiResult<List<ConfigApiResponse>>>
    {
    }
}
