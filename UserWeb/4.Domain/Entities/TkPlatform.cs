using System;
using Domain.Auditors;
using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// 平台配置，对应 tk_platform 表。
    /// 表仅有 create_time（无 is_delete），因此继承 CreateAuditor 而非 DeleteAuditor。
    /// 本实体为前台只读模型：仅由 EF Core 物化，不提供任何修改方法；
    /// 业务不变量（必须已开启）由实体自身守护，供查询 Handler 做防御校验。
    /// </summary>
    public class TkPlatform : CreateAuditor
    {
        /// <summary>
        /// 平台id（自增主键）
        /// </summary>
        public int PlatformId { get; private set; }

        /// <summary>
        /// 平台logo 图片地址
        /// </summary>
        public string PlatformImg { get; private set; } = default!;

        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; private set; } = default!;

        /// <summary>
        /// 平台状态：0 未开启 / 1 已开启
        /// </summary>
        public PlatformStatus PlatformStatus { get; private set; }

        /// <summary>
        /// 供 EF Core 物化使用。
        /// </summary>
        protected TkPlatform() { }

        /// <summary>
        /// 领域不变量：平台必须处于“已开启”状态才可被业务使用。
        /// </summary>
        public void RequiredOpened()
        {
            if (PlatformStatus != PlatformStatus.Opened)
            {
                throw new InvalidOperationException("平台未开启，无法使用！");
            }
        }
    }
}
