using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models
{
    /// <summary>
    /// 通用分页结果封装。
    /// </summary>
    public class PagedResult<T>
    {
        /// <summary>
        /// 当前页码（从 1 开始）
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 每页条数
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 当前页数据
        /// </summary>
        public IReadOnlyList<T> Items { get; set; } = new List<T>();
    }
}
