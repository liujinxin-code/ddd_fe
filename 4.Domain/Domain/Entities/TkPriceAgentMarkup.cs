using Domain.Auditors;

namespace Domain.Entities
{
    /// <summary>
    /// 代理对单业务（config）的加价金额，对应 tk_price_agent_markup 表。
    /// 加价优先级高于 tk_price_overall（代理总体加价）。
    /// 本实体为前台只读模型：仅由 EF Core 物化，不提供任何修改方法（无状态类不变量）。
    /// 约定：int/string 字段在领域模型中强制非空；decimal（价格未配置）保留可空。
    /// </summary>
    public class TkPriceAgentMarkup : CreateAuditor
    {
        /// <summary>加价id（自增主键）</summary>
        public int MarkupId { get; private set; }

        /// <summary>加价金额（decimal(10,6)）</summary>
        public decimal MarkupAddPrice { get; private set; }

        /// <summary>配置id（int）</summary>
        public int ConfigId { get; private set; }

        /// <summary>代理用户id（int）</summary>
        public int AgentUserId { get; private set; }

        /// <summary>供 EF Core 物化使用。</summary>
        protected TkPriceAgentMarkup() { }
    }
}
