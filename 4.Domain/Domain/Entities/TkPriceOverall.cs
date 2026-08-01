using System;
using Domain.Auditors;

namespace Domain.Entities
{
    /// <summary>
    /// 代理总体加价配置，对应 tk_price_overall 表。
    /// 按 userid 唯一（每代理一条）；代理未对单业务加价时，取此总体百分比加价。
    /// 代理可在前台（代理控制台）对自己这条记录进行“首次新增”与“修改”；
    /// 领域负责守护 overall_percent ∈ [0,200] 的不变量。
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

        /// <summary>创建一条总体加价配置（首次新增时调用）。</summary>
        public TkPriceOverall(long userId, int overallPercent)
        {
            UserId = userId;
            OverallPercent = overallPercent;
            RequiredValidPercent();
        }

        /// <summary>修改总体加价百分比（已存在记录时调用），会重新校验不变量。</summary>
        public void UpdatePercent(int overallPercent)
        {
            OverallPercent = overallPercent;
            RequiredValidPercent();
        }

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
