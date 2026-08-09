namespace Application.Common.Models.Response.Agent
{
    /// <summary>
    /// 代理「新增单业务加价」模态框中可选的 config 列表项。
    /// 仅返回前台可见且当前代理尚未加价的配置，并附带代理基准价供参考。
    /// </summary>
    public class AgentMarkupConfigResponse
    {
        /// <summary>配置id</summary>
        public int ConfigId { get; set; }

        /// <summary>业务/配置名称</summary>
        public string ConfigName { get; set; } = string.Empty;

        /// <summary>配置提示</summary>
        public string ConfigNotice { get; set; } = string.Empty;

        /// <summary>代理基准价（= 代理单独定价 ?? 系统底价）</summary>
        public decimal BasePrice { get; set; }

        /// <summary>前台展示价格单位（如 1000）</summary>
        public int ShowPriceUnit { get; set; }

        /// <summary>最小下单数量，0 表示无下限约束</summary>
        public int MinQuantity { get; set; }

        /// <summary>最大下单数量，0 表示无上限约束</summary>
        public int MaxQuantity { get; set; }

        /// <summary>订单数量必须被此单位整除，0 表示无整除约束</summary>
        public int OrderUnit { get; set; }
    }
}
