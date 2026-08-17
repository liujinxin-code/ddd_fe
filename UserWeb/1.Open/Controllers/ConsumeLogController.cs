using Application.Common.Models;
using Application.Common.Models.Response.ConsumeLog;
using Application.Events.ConsumeLogs.Contracts.Queries;
using MediatR;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Open.Controllers
{
    [Authorize]
    public class ConsumeLogController(IMediator mediator) : BaseController
    {
        /// <summary>
        /// 「消费流水」分页列表。仅返回当前登录用户自己的余额变动（用户id 由 JWT 注入，不可伪造）。
        /// 请求体：{ "ConsumeStatus": -1, "Keyword": "", "PageIndex": 1, "PageSize": 10, "Sorting": "createtime desc" }
        /// ConsumeStatus：-1 不筛选 / 0 订单消费 / 1 充值 / 2 代理提现(个人余额增加) /
        ///   3 转赠支出 / 4 转赠收入 / 5 订单退款 / 6 代理提现(代理收益余额减少)。
        /// Keyword 匹配流水号；排序白名单 createtime/beforeamount/afteramount/consumestatus。
        /// </summary>
        [HttpPost("list")]
        [ProducesResponseType(typeof(ApiResult<List<ConsumeLogResponse>>), StatusCodes.Status200OK)]
        public async Task<ApiResult<List<ConsumeLogResponse>>> ListAsync([FromBody] GetConsumeLogsQuery query, CancellationToken ct)
        {
            return await mediator.Send(query, ct);
        }
    }
}
