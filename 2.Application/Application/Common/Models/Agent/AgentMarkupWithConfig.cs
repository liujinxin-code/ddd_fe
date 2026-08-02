using Domain.Entities;

namespace Application.Common.Models.Agent
{
    /// <summary>
    /// 代理单业务加价记录与其关联的业务配置（仓储级读取模型）。
    /// </summary>
    public class AgentMarkupWithConfig
    {
        /// <summary>加价记录</summary>
        public TkPriceAgentMarkup Markup { get; set; } = default!;

        /// <summary>关联的业务配置</summary>
        public TkConfig Config { get; set; } = default!;
    }
}
