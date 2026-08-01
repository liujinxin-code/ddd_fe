namespace Domain.Enums;

/// <summary>
/// 公告类型，对应 tk_notice.notice_type（1 置顶公告 / 2 普通公告 / 3 弹窗公告）
/// </summary>
public enum NoticeType
{
    /// <summary>置顶公告（首页优先展示）</summary>
    Top = 1,

    /// <summary>普通公告（首页列表，按创建时间倒序）</summary>
    Normal = 2,

    /// <summary>弹窗公告（全局仅一条，前端弹窗展示）</summary>
    Popup = 3
}
