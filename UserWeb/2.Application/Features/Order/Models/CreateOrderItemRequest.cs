using System.Collections.Generic;

namespace Application.Features.Order.Models
{
    /// <summary>
    /// 批量下单中的单条明细。按业务模板(json_template)决定字段要求：
    /// - 粉丝模板(1)：OrderLink 必填 + Quantity 必填。一个链接 = 一个订单。
    /// - 评论模板(2)：OrderLink 必填 + Comments 必填。一条评论 = 一个数量，订单数量恒等于评论条数，Quantity 无需传（传了也忽略）。
    /// - 账户模板(3)：OrderLink 可留空 + Quantity = 购买账户个数。同次买多个账户算同一订单。
    /// </summary>
    public class CreateOrderItemRequest
    {
        /// <summary>业务配置id（tk_config.config_id）</summary>
        public int ConfigId { get; set; }

        /// <summary>下单链接（账户业务可留空，最长 500 字符）</summary>
        public string OrderLink { get; set; } = string.Empty;

        /// <summary>
        /// 订单数量。粉丝/账户业务必填；评论业务无需传——服务端恒以 Comments 条数为订单数量，传了也会被忽略。
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 评论内容列表，仅评论模板业务需要（其他业务传了会被忽略）。
        /// 每条最长 500 字符；一条评论 = 一个下单数量，评论条数即该订单的 quantity。
        /// </summary>
        public List<string> Comments { get; set; } = new();
    }
}
