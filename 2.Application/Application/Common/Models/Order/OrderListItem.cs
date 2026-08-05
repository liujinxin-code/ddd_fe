using System;

namespace Application.Common.Models.Order
{
    /// <summary>
    /// 订单列表项（前台「我的订单」列表用）。
    /// 平台名 / 业务类型名 / 业务名由 tk_config 关联 tk_platform、tk_platform_sub 得到；
    /// 关联数据缺失时（配置被下架删除）名称回退为空串，不影响订单本身展示。
    /// </summary>
    public class OrderListItem
    {
        /// <summary>订单序号（业务订单号）</summary>
        public string OrderNo { get; set; } = string.Empty;

        /// <summary>订单状态 1 正在执行 / 2 已完单 / 3 部分完成 / 4 已取消（未收费）</summary>
        public int OrderState { get; set; }

        /// <summary>下单链接（账户业务为空串）</summary>
        public string OrderLink { get; set; } = string.Empty;

        /// <summary>平台类型名称（如 抖音 / 快手）</summary>
        public string PlatformName { get; set; } = string.Empty;

        /// <summary>业务类型名称（原「子平台」，如 涨粉 / 评论）</summary>
        public string SubPlatformName { get; set; } = string.Empty;

        /// <summary>业务配置名称</summary>
        public string ConfigName { get; set; } = string.Empty;

        /// <summary>订单金额</summary>
        public decimal OrderAmount { get; set; }

        /// <summary>下单数量</summary>
        public int Quantity { get; set; }

        /// <summary>成功数量（履约回填）</summary>
        public int SuccessQuantity { get; set; }

        /// <summary>初始数量（= 下单时的数量，执行详情用于对比）</summary>
        public int BeginQuantity { get; set; }

        /// <summary>退费金额（默认 0）</summary>
        public decimal RefundAmount { get; set; }

        /// <summary>下单时间</summary>
        public DateTimeOffset CreateTime { get; set; }

        /// <summary>
        /// 业务模板类型 1 粉丝 / 2 评论 / 3 购买账户。
        /// 前端据此判断是否为「增量业务」（1、2）以决定是否展示执行详情。
        /// </summary>
        public int JsonTemplate { get; set; }
    }
}
