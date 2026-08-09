using System;

namespace Application.Common.Models.Response.ConsumeLog
{
    /// <summary>
    /// 消费流水列表项（前台「消费流水」列表用）。
    /// 一条记录对应 tk_consumelog 中的一行余额变动快照：变动前/后金额、变动类型、流水号。
    /// 代理提现会成对产生两条（AgentWithdrawOut 代理收益余额减少 + AgentWithdraw 个人用户余额增加），
    /// 二者 consume_no 相同，可据此关联同一笔操作的两侧。
    /// </summary>
    public class ConsumeLogResponse
    {
        /// <summary>流水主键</summary>
        public int ConsumeId { get; set; }

        /// <summary>流水号（同一笔操作的多条记录共用，如提现的转入/转出两侧）</summary>
        public string ConsumeNo { get; set; } = string.Empty;

        /// <summary>
        /// 变动类型：0 订单消费 / 1 充值 / 2 代理提现(个人余额增加) /
        /// 3 转赠支出 / 4 转赠收入 / 5 订单退款 / 6 代理提现(代理收益余额减少)
        /// </summary>
        public int ConsumeStatus { get; set; }

        /// <summary>变动前余额</summary>
        public decimal BeforeAmount { get; set; }

        /// <summary>变动后余额</summary>
        public decimal AfterAmount { get; set; }

        /// <summary>变动额 = AfterAmount - BeforeAmount（正为入账，负为扣减）</summary>
        public decimal ChangeAmount { get; set; }

        /// <summary>创建时间</summary>
        public DateTimeOffset CreateTime { get; set; }
    }
}
