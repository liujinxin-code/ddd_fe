using Domain.Auditors;

namespace Domain.Entities
{
    /// <summary>
    /// 代理对单业务（config）的加价金额，对应 tk_price_agent_markup 表。
    /// 加价优先级高于 tk_price_overall（代理总体加价）。
    /// 代理可在前台（代理控制台）对自己名下某 config 进行“新增 / 修改 / 删除”；
    /// 同一 (config_id, agent_userid) 语义上唯一：存在则修改，否则新增，删除即移除该条。
    /// 约定：int/string 字段在领域模型中强制非空；decimal（价格）保留可空。
    /// </summary>
    public class TkPriceAgentMarkup : CreateAuditor
    {
        /// <summary>加价id（自增主键）</summary>
        public int MarkupId { get; private set; }

        /// <summary>加价金额（decimal(10,6)）</summary>
        public decimal MarkupAddPrice { get; private set; }

        /// <summary>配置id（int）</summary>
        public int ConfigId { get; private set; }

        /// <summary>代理用户id（bigint）</summary>
        public long AgentUserId { get; private set; }

        /// <summary>供 EF Core 物化使用。</summary>
        protected TkPriceAgentMarkup() { }

        /// <summary>创建一条单业务加价记录（首次新增时调用）。</summary>
        public TkPriceAgentMarkup(int configId, long agentUserId, decimal markupAddPrice)
        {
            ConfigId = configId;
            AgentUserId = agentUserId;
            MarkupAddPrice = markupAddPrice;
        }

        /// <summary>修改加价金额（已存在记录时调用）。</summary>
        public void UpdateMarkup(decimal markupAddPrice)
        {
            MarkupAddPrice = markupAddPrice;
        }
    }
}
