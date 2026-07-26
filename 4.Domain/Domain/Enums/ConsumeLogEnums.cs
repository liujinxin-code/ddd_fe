using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    /// <summary>用户 user_amount 余额变化类型。</summary>
    public enum ConsumeStatus
    {
        /// <summary>
        /// 订单
        /// </summary>
        OrderConsume = 0,
        /// <summary>
        /// 充值
        /// </summary>
        Recharge = 1,
        /// <summary>
        /// 代理提现
        /// </summary>
        AgentWithdraw = 2,
        /// <summary>
        /// 代理转出
        /// </summary>
        AgentTransferOut = 3,
        /// <summary>
        /// 代理转入
        /// </summary>
        AgentTransferIn = 4,
        /// <summary>
        /// 订单退款
        /// </summary>
        OrderRefund = 5,
        /// <summary>
        /// 管理员调整
        /// </summary>
        AdminAdjustment = 6
    }
}
