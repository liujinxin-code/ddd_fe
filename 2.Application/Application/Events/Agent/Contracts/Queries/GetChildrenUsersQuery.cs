using Application.Common.Models;
using Application.Common.Models.Agent;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Events.Agent.Contracts.Queries
{
    /// <summary>
    /// 代理分页查询自己的下级用户。
    /// AgentUserid 由控制器从当前登录代理注入，前台不可伪造。
    /// Keyword 为可选关键词，按用户名或邮箱模糊匹配（空则表示查全部）。
    /// 排序字段 Sorting 形如 "useramount desc" / "username asc"，缺省按 userid 倒序。
    /// </summary>
    public record class GetChildrenUsersQuery(
        long AgentUserid = 0,
        int PageIndex = 1,
        int PageSize = 20,
        string? Keyword = null,
        string? Sorting = null
    ) : IRequest<ApiResult<PagedResult<ChildrenUserListItem>>>
    {
    }
}
