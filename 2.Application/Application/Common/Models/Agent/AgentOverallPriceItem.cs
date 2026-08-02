using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models.Agent
{
    /// <summary>
    /// 代理当前总体加价百分比（tk_price_overall）。
    /// 未设置时 OverallPercent 为 0。
    /// </summary>
    public class AgentOverallPriceItem
    {
        /// <summary>
        /// 总体加价百分比（0-200）。
        /// </summary>
        public int OverallPercent { get; set; }
    }
}
