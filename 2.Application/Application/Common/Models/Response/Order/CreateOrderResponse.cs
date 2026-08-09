using System.Collections.Generic;

namespace Application.Common.Models.Response.Order
{
    /// <summary>
    /// 批量下单结果：返回本次创建的全部订单号与总扣款金额（已按当前用户定价计算）。
    /// </summary>
    public class CreateOrderResponse
    {
        /// <summary>本次创建成功的订单号列表（顺序与请求明细一致）</summary>
        public List<string> OrderNos { get; set; } = new();

        /// <summary>总扣款金额（所有明细订单金额之和）</summary>
        public decimal TotalAmount { get; set; }
    }
}
