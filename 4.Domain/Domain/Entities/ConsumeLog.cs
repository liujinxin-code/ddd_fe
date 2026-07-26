using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{

    /// <summary>用户余额变化流水，对应 tk_consumelog。</summary>
    public sealed class ConsumeLog
    {
        private ConsumeLog() { }

        /// <summary>创建一条不可变的余额变化快照。</summary>
        public ConsumeLog(long userId, decimal beforeAmount, decimal afterAmount, ConsumeStatus status, string consumeNo)
        {
            UserId = userId;
            BeforeAmount = beforeAmount;
            AfterAmount = afterAmount;
            ConsumeStatus = status;
            ConsumeNo = consumeNo;
            CreateTime = DateTime.UtcNow;
        }

        public int ConsumeId { get; private set; }
        /// <summary>
        /// 操作前金额
        /// </summary>
        public decimal BeforeAmount { get; private set; }
        /// <summary>
        /// 操作后金额
        /// </summary>
        public decimal AfterAmount { get; private set; }
        /// <summary>
        /// 日志类型
        /// </summary>
        public ConsumeStatus ConsumeStatus { get; private set; }
        /// <summary>
        /// 消费日志订单号
        /// </summary>
        public string ConsumeNo { get; private set; } = string.Empty;
        public long UserId { get; private set; }
        public DateTime? CreateTime { get; private set; }
    }
}
