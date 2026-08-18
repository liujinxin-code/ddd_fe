using Application.Common.Models;
using Application.Features.Agent.Models;
using MediatR;

namespace Application.Features.Agent
{
    /// <summary>
    /// 代理分页查询自己的下级用户。
    /// Keyword 为可选关键词，按用户名或邮箱模糊匹配（空则表示查全部）。
    /// UserStatus 为可选状态筛选（null=全部，1=启用，0=停用），对应 TkUserStatus 枚举。
    /// 排序字段 Sorting 形如 "useramount desc" / "username asc"，缺省按 userid 倒序。
    /// 分页/排序参数（PageIndex / PageSize / Sorting）继承自 PagedQuery。
    /// 返回当前页下级用户列表（PagedResult&lt;ChildrenUserResponse&gt;）。页码/页大小已在请求中，无需回显；真实总条数由 PagedResult.TotalCount 携带，在 HTTP 边缘层（ApiPaged）统一包装为信封 DataTotal。
    /// </summary>
    public record class GetChildrenUsersQuery(
        string? Keyword = null,
        int? UserStatus = null
    ) : PagedQuery, IRequest<PagedResult<ChildrenUserResponse>>
    {
    }
}
