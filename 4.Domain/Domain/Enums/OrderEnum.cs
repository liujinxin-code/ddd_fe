namespace Domain.Enums;

/// <summary>
/// 订单状态，对应 tk_order.order_state（1 正在执行 / 2 已完单 / 3 部分完成 / 4 已取消）
/// </summary>
public enum OrderState
{
    /// <summary>正在执行（下单成功，履约进行中）</summary>
    Running = 1,

    /// <summary>已完单（全量完成）</summary>
    Completed = 2,

    /// <summary>部分完成（成功数量小于下单数量，差额部分退费）</summary>
    PartiallyCompleted = 3,

    /// <summary>已取消（未收费）</summary>
    Cancelled = 4
}
