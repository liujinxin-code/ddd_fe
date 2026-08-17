using System;
using Domain.Auditors;
using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// 业务类型，对应 tk_platform_sub 表。
/// 从属于某个平台（platform_id 指向 tk_platform.platform_id）。
/// 表仅有 create_time（无 is_delete），因此继承 CreateAuditor 而非 DeleteAuditor。
/// 本实体为前台只读模型：仅由 EF Core 物化，不提供任何修改方法；
/// 业务不变量（必须启用、归属一致）由实体自身守护，供查询 Handler 做防御校验。
/// </summary>
public class TkPlatformSub : CreateAuditor
{
    /// <summary>
    /// 业务类型id（自增主键）
    /// </summary>
    public int SubPlatformId { get; private set; }

    /// <summary>
    /// 业务类型名称
    /// </summary>
    public string SubPlatformName { get; private set; } = default!;

    /// <summary>
    /// 所属平台id（对应 tk_platform.platform_id）
    /// </summary>
    public int PlatformId { get; private set; }

    /// <summary>
    /// 业务类型状态：0 停用 / 1 启用
    /// </summary>
    public SubPlatformStatus SubPlatformStatus { get; private set; }

    /// <summary>
    /// 侧边栏公告：前台用户选中该业务类型时展示（对应 sub_platform_notice）。
    /// </summary>
    public string SubPlatformNotice { get; private set; } = default!;

    /// <summary>
    /// 供 EF Core 物化使用。
    /// </summary>
    protected TkPlatformSub() { }

    /// <summary>
    /// 领域不变量：业务类型必须处于“启用”状态才可被业务使用。
    /// </summary>
    public void RequiredEnabled()
    {
        if (SubPlatformStatus != SubPlatformStatus.Enabled)
        {
            throw new InvalidOperationException("业务类型未启用，无法使用！");
        }
    }

    /// <summary>
    /// 防御深度：校验业务类型确实归属于指定平台。
    /// </summary>
    public void RequiredBelongsTo(int platformId)
    {
        if (PlatformId != platformId)
        {
            throw new InvalidOperationException("业务类型归属平台不一致");
        }
    }
}
