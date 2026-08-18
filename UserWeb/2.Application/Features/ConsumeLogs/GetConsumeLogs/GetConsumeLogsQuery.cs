using Application.Common.Models;
using Application.Features.ConsumeLogs.Models;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.ConsumeLogs
{
    /// <summary>
    /// 分页查询「消费流水」列表。仅返回当前登录用户自己的余额变动记录。
    /// 当前登录用户id 由 ICurrentUser 注入，契约不含用户id，前台不可伪造。
    /// 排序白名单：createtime / beforeamount / afteramount / consumestatus，缺省按时间倒序。
    /// </summary>
    public record class GetConsumeLogsQuery(
        /// <summary>变动类型筛选：-1 表示全部；0-6 对应 ConsumeStatus 枚举各值</summary>
        int ConsumeStatus = -1,
        string? Keyword = null
    ) : PagedQuery, IRequest<PagedResult<ConsumeLogResponse>>;
}
