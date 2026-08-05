using Application.Common.Models.Order;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Abstractions.Repositories
{
    /// <summary>
    /// 订单仓储。下单时批量落库 TkOrder；列表查询走投影读模型（联表取平台/业务类型名称），避免 N+1。
    /// </summary>
    public interface IOrderRepository : IRepository<TkOrder>
    {
        /// <summary>
        /// 批量新增订单（用于整批原子下单）。
        /// </summary>
        Task AddRangeAsync(IEnumerable<TkOrder> orders, CancellationToken ct = default);

        /// <summary>
        /// 分页查询指定用户的订单列表，左连 tk_config / tk_platform / tk_platform_sub 补齐名称。
        /// orderState 传 0 表示不按状态过滤；keyword 匹配订单号或下单链接。
        /// 排序字段来自白名单（createtime/orderamount/quantity/orderstate），缺省按下单时间倒序。
        /// </summary>
        Task<(IReadOnlyList<OrderListItem> Items, int Total)> GetPagedByUserAsync(
            int userId,
            int orderState,
            string? keyword,
            int pageIndex,
            int pageSize,
            string? sortField,
            bool sortDesc,
            CancellationToken ct = default);
    }
}
