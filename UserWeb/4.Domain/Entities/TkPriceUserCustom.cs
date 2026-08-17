using Domain.Auditors;

namespace Domain.Entities
{
    /// <summary>
    /// 管理员对用户+配置的单品单独定价，对应 tk_price_user_custom 表。
    /// userid 既可为普通用户也可为代理；作为代理的“进货价”来源之一。
    /// 本实体为前台只读模型：仅由 EF Core 物化，不提供任何修改方法（无状态类不变量）。
    /// 约定：int/string 字段在领域模型中强制非空；decimal（价格未配置）保留可空。
    /// </summary>
    public class TkPriceUserCustom : CreateAuditor
    {
        /// <summary>主键id（自增）</summary>
        public int Id { get; private set; }

        /// <summary>自定义价格（decimal(10,6)）</summary>
        public decimal CustomPrice { get; private set; }

        /// <summary>用户id（int）</summary>
        public int UserId { get; private set; }

        /// <summary>配置id（int）</summary>
        public int ConfigId { get; private set; }

        /// <summary>供 EF Core 物化使用。</summary>
        protected TkPriceUserCustom() { }
    }
}
