using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Auditors
{
    /// <summary>
    /// 删除审计
    /// </summary>
    public class DeleteAuditor : CreateAuditor
    {
        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { get; set; }

        /// <summary>
        /// 删除时间；null 表示未删除。
        /// 注意：这里不能用 DateTimeOffset.MinValue 当默认值——0001-01-01 低于 MySQL DATETIME
        /// 的合法下限（1000-01-01），严格模式下会直接插入失败。
        /// </summary>
        public DateTimeOffset? DeleteTime { get; set; }

        /// <summary>
        /// 软删除：置删除标记并记录删除时间。
        /// 幂等——重复调用不会覆盖首次删除时间，避免审计信息被冲掉。
        /// </summary>
        public void MarkDeletedFunc()
        {
            if (IsDelete) return;

            IsDelete = true;
            DeleteTime = DateTimeOffset.Now;
        }
    }
}
