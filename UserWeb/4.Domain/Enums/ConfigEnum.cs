namespace Domain.Enums;

/// <summary>
/// 业务配置状态，对应 tk_config.config_status（0 未启用 / 1 全部启用 / 2 仅限 API）
/// </summary>
public enum ConfigStatus
{
    /// <summary>未启用</summary>
    Disabled = 0,

    /// <summary>全部启用（WEB + API 均可使用）</summary>
    AllEnabled = 1,

    /// <summary>仅限 API（不向前台展示，仅供 API 下单）</summary>
    ApiOnly = 2
}

/// <summary>
/// 业务配置模板类型，对应 tk_config.json_template（1 粉丝模板 / 2 评论模板 / 3 购买账户模板）
/// </summary>
public enum JsonTemplate
{
    /// <summary>粉丝模板</summary>
    Follower = 1,

    /// <summary>评论模板</summary>
    Comment = 2,

    /// <summary>购买账户模板</summary>
    PurchaseAccount = 3
}
