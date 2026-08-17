using System.ComponentModel;

namespace Domain.Enums;

/// <summary>工单状态，对应 tk_ticket.tikcket_status（0 待处理 / 1 已处理）</summary>
public enum TicketStatus
{
    [Description("待处理")]
    Pending = 0,

    [Description("已处理")]
    Processed = 1,
}

/// <summary>工单问题类型，对应 tk_ticket.ticket_type（0 订单问题 / 1 下单问题 / 2 网站问题 / 3 网站建议）</summary>
public enum TicketType
{
    [Description("订单问题")]
    Order = 0,

    [Description("下单问题")]
    OrderCreate = 1,

    [Description("网站问题")]
    Website = 2,

    [Description("网站建议")]
    Suggestion = 3,
}
