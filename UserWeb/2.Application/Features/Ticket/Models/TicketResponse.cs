using System;
using System.Collections.Generic;

namespace Application.Features.Ticket.Models
{
    /// <summary>工单列表 DTO，投影自 TkTicket，images 已反序列化为字符串列表。</summary>
    public class TicketResponse
    {
        public int TicketId { get; set; }

        /// <summary>工单编号（后端生成，用于展示与检索）</summary>
        public string TicketNo { get; set; } = string.Empty;

        public string TicketContent { get; set; } = string.Empty;

        /// <summary>工单图片相对 URL 列表（最多 5 张）</summary>
        public List<string> TicketImages { get; set; } = new();

        /// <summary>后台处理结果</summary>
        public string TicketResult { get; set; } = string.Empty;

        /// <summary>工单状态：0 待处理 / 1 已处理</summary>
        public int TicketStatus { get; set; }

        /// <summary>问题类型：0 订单问题 / 1 下单问题 / 2 网站问题 / 3 网站建议</summary>
        public int TicketType { get; set; }

        public long Userid { get; set; }

        public DateTimeOffset CreateTime { get; set; }
    }
}
