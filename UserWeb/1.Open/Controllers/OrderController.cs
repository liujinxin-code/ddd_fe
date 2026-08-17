using Application.Common.Models;
using Application.Features.Order.Models;
using Application.Features.Order;
using MediatR;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Open.Controllers
{
    [Authorize]
    public class OrderController(IMediator mediator) : BaseController
    {
        /// <summary>
        /// 批量下单。
        /// 请求体：{ "Items": [ { "ConfigId": 1, "OrderLink": "https://...", "Quantity": 1000 } ] }
        /// 增量业务（涨粉/评论）：一个链接 = 一个订单，OrderLink 必填。
        /// 账户业务（买号）：Quantity = 购买账户个数，多个账户算同一订单，OrderLink 选填。
        /// 整批原子：任一明细失败（配置无效/未启用/数量越界/余额不足等）则全部不创建并回滚。
        /// 当前登录用户由 JWT 注入（ICurrentUser），调用方不可伪造。
        /// 注：该 API 后续会开放给用户自行调用；如改用 API Key 鉴权，仅需调整 CurrentUserAccessor 的解析来源，契约不变。
        /// </summary>
        [HttpPost("create")]
        [ProducesResponseType(typeof(ApiResult<CreateOrderResponse>), StatusCodes.Status200OK)]
        public async Task<ApiResult<CreateOrderResponse>> CreateAsync([FromBody] CreateOrdersCommand cmd, CancellationToken ct)
        {
            return await mediator.Send(cmd, ct);
        }

        /// <summary>
        /// 「我的订单」分页列表。仅返回当前登录用户自己的订单（用户id 由 JWT 注入，不可伪造）。
        /// 请求体：{ "OrderState": 0, "Keyword": "", "PageIndex": 1, "PageSize": 20, "Sorting": "createtime desc" }
        /// OrderState：0 不筛选 / 1 正在执行 / 2 已完单 / 3 部分完成 / 4 已取消（未收费）。
        /// Keyword 匹配订单号或下单链接；排序白名单 createtime/orderamount/quantity/orderstate。
        /// </summary>
        [HttpPost("list")]
        [ProducesResponseType(typeof(ApiResult<List<OrderResponse>>), StatusCodes.Status200OK)]
        public async Task<ApiResult<List<OrderResponse>>> ListAsync([FromBody] GetOrdersQuery query, CancellationToken ct)
        {
            return await mediator.Send(query, ct);
        }
    }
}
