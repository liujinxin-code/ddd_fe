using Application.Common.Models;
using Application.Features.Agent.Models;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Agent
{
    /// <summary>
    /// 代理获取自己名下单业务加价记录列表（tk_price_agent_markup）。
    /// 支持按业务名称关键字检索、分页排序。
    /// 当前登录用户id 由 ICurrentUser 注入（前台不可伪造）。
    /// </summary>
    public record class GetAgentMarkupsQuery(
        string Keyword = ""
    ) : PagedQuery, IRequest<ApiResult<List<AgentMarkupResponse>>>;
}
