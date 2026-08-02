using System;

namespace Application.Common.Models.Agent
{
    /// <summary>
    /// 代理单业务加价列表项（tk_price_agent_markup + tk_config 关联）。
    /// 用于代理管理页「业务加价」卡片列表展示。
    /// </summary>
    public class AgentMarkupListItem
    {
        /// <summary>加价记录id</summary>
        public int MarkupId { get; set; }

        /// <summary>配置id</summary>
        public int ConfigId { get; set; }

        /// <summary>业务/配置名称</summary>
        public string ConfigName { get; set; } = string.Empty;

        /// <summary>配置提示</summary>
        public string ConfigNotice { get; set; } = string.Empty;

        /// <summary>系统底价（成本价，数量单个）</summary>
        public decimal ConfigPrice { get; set; }

        /// <summary>代理基准价（= 代理单独定价 ?? 系统底价）</summary>
        public decimal BasePrice { get; set; }

        /// <summary>代理对该业务的加价金额</summary>
        public decimal MarkupAddPrice { get; set; }

        /// <summary>展示价格单位（如 1000）</summary>
        public int ShowPriceUnit { get; set; }

        /// <summary>下级用户看到的前台展示价 = ShowPriceUnit × (BasePrice + MarkupAddPrice)</summary>
        public decimal ChildDisplayPrice { get; set; }

        /// <summary>记录创建时间</summary>
        public DateTimeOffset CreateTime { get; set; }
    }
}
