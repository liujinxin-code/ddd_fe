using Application.Common.Models;
using Application.Common.Models.Agent;
using MediatR;
using System.Collections.Generic;

namespace Application.Events.Agent.Contracts.Queries
{
    /// <summary>
    /// 代理在「新增单业务加价」模态框中，按平台/子平台获取可选择的 config 列表。
    /// 仅返回前台可见且当前代理尚未加价的配置，并附带代理基准价。
    /// 当前登录用户id 由 ICurrentUser 注入。
    /// </summary>
    public record class GetAgentMarkupConfigsQuery(
        int PlatformId = 0,
        int SubPlatformId = 0
    ) : PagedQuery, IRequest<ApiResult<List<AgentMarkupConfigItem>>>;
}
