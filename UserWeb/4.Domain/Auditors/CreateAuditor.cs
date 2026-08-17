using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Auditors
{
    /// <summary>
    /// 创建审计
    /// </summary>
    public class CreateAuditor
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.Now;
    }
}
