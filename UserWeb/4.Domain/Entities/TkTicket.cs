using System;

namespace Domain.Entities
{
    /// <summary>
    /// 客服工单，对应 tk_ticket。
    /// 用户提交问题（订单/下单/网站问题/建议）+ 图片，后台处理并填写 ticket_result。
    /// 注意：数据库列名 `tikcket_status` 是建表时的拼写（缺少一个 c），这里保持列名映射不变。
    /// </summary>
    public sealed class TkTicket
    {
        private TkTicket() { }

        /// <summary>
        /// 创建一条待处理工单。images 为图片相对 URL 列表（已上传到静态目录）。
        /// ticketNo 为工单编号（后端生成，如 T + 时间戳 + 随机），用于前台展示与检索。
        /// </summary>
        public TkTicket(long userid, string ticketNo, string ticketContent, int ticketType, string ticketImages)
        {
            Userid = userid;
            TicketNo = ticketNo;
            TicketContent = ticketContent;
            TicketType = ticketType;
            TicketImages = ticketImages;
            TicketResult = string.Empty;
            TicketStatus = 0; // 0 = 待处理
            CreateTime = DateTimeOffset.UtcNow;
        }

        public int TicketId { get; private set; }

        /// <summary>工单编号（后端生成，如 T + 时间戳 + 随机），用于前台展示与检索</summary>
        public string TicketNo { get; private set; } = default!;

        /// <summary>工单内容</summary>
        public string TicketContent { get; private set; } = default!;

        /// <summary>工单图片，JSON 数组字符串：["/images/20260806/xxx.png", ...]，最多 5 张</summary>
        public string TicketImages { get; private set; } = default!;

        /// <summary>处理结果（后台填写）</summary>
        public string TicketResult { get; private set; } = default!;

        /// <summary>工单状态：0 待处理 / 1 已处理（对应 tk_ticket.tikcket_status 列）</summary>
        public int TicketStatus { get; private set; }

        /// <summary>创建时间（UTC）</summary>
        public DateTimeOffset CreateTime { get; private set; }

        /// <summary>问题类型：0 订单问题 / 1 下单问题 / 2 网站问题 / 3 网站建议</summary>
        public int TicketType { get; private set; }

        /// <summary>提交用户 id</summary>
        public long Userid { get; private set; }
    }
}
