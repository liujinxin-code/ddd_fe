namespace Application.Features.Agent.Models
{
    /// <summary>
    /// 代理当前总体加价百分比（tk_price_overall）。
    /// 未设置时 OverallPercent 为 0。
    /// </summary>
    public class AgentOverallPriceResponse
    {
        /// <summary>
        /// 总体加价百分比（0-200）。
        /// </summary>
        public int OverallPercent { get; set; }
    }
}
