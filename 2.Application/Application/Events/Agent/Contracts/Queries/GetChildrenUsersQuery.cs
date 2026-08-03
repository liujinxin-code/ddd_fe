using Application.Common.Models;
using Application.Common.Models.Agent;
using MediatR;

namespace Application.Events.Agent.Contracts.Queries
{
    /// <summary>
    /// 代理分页查询自己的下级用户。
    /// Keyword 为可选关键词，按用户名或邮箱模糊匹配（空则表示查全部）。
    /// 排序字段 Sorting 形如 "useramount desc" / "username asc"，缺省按 userid 倒序。
    /// 分页/排序参数（PageIndex / PageSize / Sorting）继承自 PagedQuery。
    /// 返回当前页下级用户列表（ApiResult&lt;List&lt;ChildrenUserListItem&gt;&gt;），页码/页大小已在请求中，无需回显；总条数通过 ApiResult.DataTotal 返回。
    /// </summary>
    public record class GetChildrenUsersQuery(
        string? Keyword = null
    ) : PagedQuery, IRequest<ApiResult<List<ChildrenUserListItem>>>
    {
    }
}
