using Application.Common.Models;
using Application.Features.Config.Models;
using MediatR;

namespace Application.Features.Config
{
    /// <summary>
    /// 首页：用户选定平台与业务类型后，分页获取该业务类型下“前台可见”的业务配置，并附带“当前用户看到的价格”。
    /// 排序字段 Sorting 形如 "configprice desc" / "configsort asc"，缺省按 config_sort 升序。
    /// 分页/排序参数（PageIndex / PageSize / Sorting）继承自 PagedQuery。
    /// 返回当前页业务列表（PagedResult&lt;ConfigResponse&gt;）。页码/页大小已在请求中，无需回显；真实总条数由 PagedResult.TotalCount 携带，在 HTTP 边缘层（ApiPaged）统一包装为信封 DataTotal。
    /// </summary>
    public record class GetConfigsQuery(
        int PlatformId = 0,
        int SubPlatformId = 0,
        string Keyword = ""
    ) : PagedQuery, IRequest<PagedResult<ConfigResponse>>
    {
    }
}
