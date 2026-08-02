using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models.Agent
{
    /// <summary>
    /// 代理管理页顶部仪表盘数据：用户余额、代理余额、下级用户启用数/总数。
    /// </summary>
    public class AgentDashboardItem
    {
        /// <summary>
        /// 用户余额（当前登录用户）
        /// </summary>
        public decimal UserAmount { get; set; }

        /// <summary>
        /// 代理收益余额
        /// </summary>
        public decimal AgentAmount { get; set; }

        /// <summary>
        /// 已启用的下级用户数
        /// </summary>
        public int EnabledChildrenCount { get; set; }

        /// <summary>
        /// 下级用户总数
        /// </summary>
        public int TotalChildrenCount { get; set; }
    }
}
