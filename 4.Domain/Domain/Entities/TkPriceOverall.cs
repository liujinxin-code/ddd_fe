using System;
using Domain.Auditors;

namespace Domain.Entities
{
    /// <summary>
    /// 代理总体加价配置，对应 tk_price_overall 表。
    /// 按 userid 唯一（每代理一条）；代理未对单业务加价时，取此总体百分比加价。
    /// 本实体为前台只读模型：仅由 EF Core 物化，不提供任何修改方法。
    /// 约定：int/string 字段在领域模型中强制非空；decimal/时间 保留可空。
    /// </summary>
    public class TkPriceOverall : CreateAuditor
    {
        /// <summary>总体加价id（自增主键）</summary>
        public int OverallId { get; private set; }

        /// <summary>总体加价百分比，取值范围 [0,200]</summary>
        public int OverallPercent { get; private set; }

        /// <summary>代理用户id（bigint）</summary>
        public long UserId { get; private set; }

        /// <summary>供 EF Core 物化使用。</summary>
        protected TkPriceOverall() { }

        /// <summary>
        /// 领域不变量：总体加价百分比须落在 [0,200]。
        /// </summary>
        public void RequiredValidPercent()
        {
            if (OverallPercent < 0 || OverallPercent > 200)
            {
                throw new InvalidOperationException("总体加价百分比须在 [0,200] 之间！");
            }
        }
    }
}
