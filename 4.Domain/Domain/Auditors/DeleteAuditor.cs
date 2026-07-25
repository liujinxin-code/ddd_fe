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
    }
}
