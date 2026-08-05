using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Common.Models.Order;
using Application.Events.Order.Contracts.Queries;
using MediatR;
using Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Events.Order.Handlers.Queries
{
    /// <summary>
    /// 「我的订单」分页查询。只读当前登录用户自己的订单（用户id 来自 ICurrentUser，杜绝越权查询他人订单）。
    /// 平台名/业务类型名由仓储侧一次左连带出，避免逐条回查。
    /// </summary>
    public class GetOrdersQueryHandler(
        IOrderRepository orderRepository,
        ICurrentUser currentUser)
        : IRequestHandler<GetOrdersQuery, ApiResult<List<OrderListItem>>>
    {
        public async Task<ApiResult<List<OrderListItem>>> Handle(GetOrdersQuery query, CancellationToken ct)
        {
            if (!currentUser.IsAuthenticated || currentUser.Userid <= 0)
            {
                return new ApiResult<List<OrderListItem>>
                {
                    Code = 401,
                    Message = "登录失效",
                    Data = new List<OrderListItem>(),
                    DataTotal = 0
                };
            }

            string sortField;
            bool sortDesc;
            if (string.IsNullOrWhiteSpace(query.Sorting))
            {
                sortField = "createtime";
                sortDesc = true;
            }
            else
            {
                var parts = query.Sorting.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                sortField = parts[0];
                sortDesc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
            }

            var (items, total) = await orderRepository.GetPagedByUserAsync(
                (int)currentUser.Userid,
                query.OrderState,
                query.Keyword,
                query.PageIndex,
                query.PageSize,
                sortField,
                sortDesc,
                ct);

            // 金额统一按 decimal(11,6) 精度回传，避免前端拿到未规整的小数尾巴。
            var list = items.Select(o =>
            {
                o.OrderAmount = Utils.RoundToSixDecimals(o.OrderAmount);
                o.RefundAmount = Utils.RoundToSixDecimals(o.RefundAmount);
                return o;
            }).ToList();

            return new ApiResult<List<OrderListItem>>
            {
                Code = 200,
                Message = "Success!",
                Data = list,
                DataTotal = total
            };
        }
    }
}
