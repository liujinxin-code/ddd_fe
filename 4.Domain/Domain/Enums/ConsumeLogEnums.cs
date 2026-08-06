using System.ComponentModel;

namespace Domain.Enums
{
    /// <summary>用户 user_amount 余额变化类型。</summary>
    public enum ConsumeStatus
    {
        /// <summary>订单消费</summary>
        [Description("订单消费")]
        OrderConsume = 0,

        /// <summary>充值</summary>
        [Description("充值")]
        Recharge = 1,

        /// <summary>代理提现 - 代理收益余额减少（提现来源侧，与 AgentWithdraw 成对，共用同一流水号）</summary>
        [Description("代理收益扣减")]
        AgentWithdrawOut = 6,

        /// <summary>代理提现（到账侧，个人用户余额增加）</summary>
        [Description("代理收益提现")]
        AgentWithdraw = 2,


        /// <summary>转赠支出</summary>
        [Description("转赠支出")]
        AgentTransferOut = 3,

        /// <summary>转赠收入</summary>
        [Description("转赠收入")]
        AgentTransferIn = 4,

        /// <summary>订单退款</summary>
        [Description("订单退款")]
        OrderRefund = 5,


    }
}
