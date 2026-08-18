using Application.Features.Order.Models;
using Application.Common.Models;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Order
{
    /// <summary>
    /// 批量下单命令。
    /// 当前登录用户由 ICurrentUser 注入（[Authorize] + CurrentUserAccessor），调用方不可伪造。
    /// 整批原子：任一明细失败（配置不存在/未启用/数量越界/余额不足等）则整批不创建并回滚。
    ///
    /// 请求体示例：
    /// {
    ///   "Items": [
    ///     { "ConfigId": 1, "OrderLink": "https://x.com/xxx", "Quantity": 1000 },
    ///     { "ConfigId": 2, "Quantity": 5 }
    ///   ]
    /// }
    ///
    /// 说明：
    /// - 增量业务（涨粉/评论）：一个链接 = 一个订单，OrderLink 必填。
    /// - 账户业务（买号）：Quantity = 购买账户个数，多个账户算同一订单，OrderLink 选填。
    /// </summary>
    public record CreateOrdersCommand(List<CreateOrderItemRequest> Items) : IRequest<CreateOrderResponse>;
}
