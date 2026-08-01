using System;
using Domain.Auditors;
using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// 业务配置（商品/SKU），对应 tk_config 表。
    /// 表仅有 create_time（无 is_delete），因此继承 CreateAuditor 而非 DeleteAuditor。
    /// 本实体为前台只读模型：仅由 EF Core 物化，不提供任何修改方法；
    /// 业务不变量（启用状态、前台可见、下单数量约束）由实体自身守护，供查询/下单 Handler 做防御校验。
    /// 约定：int/string 字段在领域模型中强制非空（不向数据库 DEFAULT NULL 妥协），
    /// 约束类字段（Min/Max/OrderUnit）以 0 表达“无约束”；价格(decimal)、时间(DateTime) 保留可空表示未配置/未设。
    /// </summary>
    public class TkConfig : CreateAuditor
    {
        /// <summary>配置id（自增主键）</summary>
        public int ConfigId { get; private set; }

        /// <summary>配置名称</summary>
        public string ConfigName { get; private set; } = default!;

        /// <summary>系统底价（数量单个），decimal(10,6)</summary>
        public decimal ConfigPrice { get; private set; }

        /// <summary>前台展示价格单位（展示价 = show_price_unit × 用户最终单价，如“1000个/50元”）</summary>
        public int ShowPriceUnit { get; private set; }

        /// <summary>订单数量必须被此单位整除，0 表示无整除约束</summary>
        public int OrderUnit { get; private set; }

        /// <summary>配置提示</summary>
        public string ConfigNotice { get; private set; } = default!;

        /// <summary>平台id</summary>
        public int PlatformId { get; private set; }

        /// <summary>子平台id</summary>
        public int SubPlatformId { get; private set; }

        /// <summary>渠道id（不参与价格计算，仅作分类/筛选）</summary>
        public int ChannelId { get; private set; }

        /// <summary>渠道服务id（不参与价格计算，仅作分类/筛选）</summary>
        public int ChannelServerId { get; private set; }

        /// <summary>最小下单数量，0 表示无下限约束</summary>
        public int MinQuantity { get; private set; }

        /// <summary>最大下单数量，0 表示无上限约束</summary>
        public int MaxQuantity { get; private set; }

        /// <summary>排序</summary>
        public int ConfigSort { get; private set; }

        /// <summary>配置状态：0 未启用 / 1 全部启用 / 2 仅限 API</summary>
        public ConfigStatus ConfigStatus { get; private set; }

        /// <summary>模板类型：1 粉丝模板 / 2 评论模板 / 3 购买账户模板</summary>
        public JsonTemplate JsonTemplate { get; private set; }

        /// <summary>供 EF Core 物化使用。</summary>
        protected TkConfig() { }

        /// <summary>
        /// 领域不变量：配置须处于启用状态（API 侧可用，含“仅限 API”）。
        /// </summary>
        public void RequiredEnabled()
        {
            if (ConfigStatus == Domain.Enums.ConfigStatus.Disabled)
            {
                throw new InvalidOperationException("配置未启用，无法使用！");
            }
        }

        /// <summary>
        /// 领域不变量：配置须对前台（WEB）可见，即状态为“全部启用(1)”。
        /// “仅限 API(2)”的配置不向前台展示，仅供 API 下单。
        /// </summary>
        public void RequiredWebVisible()
        {
            if (ConfigStatus != Domain.Enums.ConfigStatus.AllEnabled)
            {
                throw new InvalidOperationException("配置不向前台展示！");
            }
        }

        /// <summary>
        /// 领域不变量：校验下单数量满足约束（min ≤ qty ≤ max 且 qty 可被 order_unit 整除）。
        /// 约束字段为 0 时视为无该约束。
        /// </summary>
        public void RequiredQuantity(int quantity)
        {
            if (MinQuantity > 0 && quantity < MinQuantity)
            {
                throw new InvalidOperationException($"下单数量不能小于 {MinQuantity}！");
            }
            if (MaxQuantity > 0 && quantity > MaxQuantity)
            {
                throw new InvalidOperationException($"下单数量不能大于 {MaxQuantity}！");
            }
            if (OrderUnit > 0 && quantity % OrderUnit != 0)
            {
                throw new InvalidOperationException($"下单数量必须是 {OrderUnit} 的整数倍！");
            }
        }
    }
}
