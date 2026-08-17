using Application.Common.Models;
using Application.Features.Order.Models;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Order
{
    /// <summary>
    /// 分页查询「我的订单」列表。仅返回当前登录用户自己的订单。
    /// 当前登录用户id 由 ICurrentUser 注入，契约不含用户id，前台不可伪造。
    /// 排序白名单：createtime / orderamount / quantity / orderstate，缺省按下单时间倒序。
    /// </summary>
    public record class GetOrdersQuery(
        int OrderState = 0,
        string? Keyword = null
    ) : PagedQuery, IRequest<ApiResult<List<OrderResponse>>>;
}
