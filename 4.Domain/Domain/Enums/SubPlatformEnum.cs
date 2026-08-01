namespace Domain.Enums;

/// <summary>
/// 子平台状态：0 停用 / 1 启用（对应 tk_platform_sub.sub_platform_status）
/// </summary>
public enum SubPlatformStatus
{
    /// <summary>
    /// 停用
    /// </summary>
    Disabled = 0,

    /// <summary>
    /// 启用
    /// </summary>
    Enabled = 1
}
