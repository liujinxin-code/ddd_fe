using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    /// <summary>
    /// 平台启停状态，对应 tk_platform.platform_state（0 未开启 / 1 已开启）
    /// </summary>
    public enum PlatformStatus
    {
        /// <summary>
        /// 未开启
        /// </summary>
        Closed = 0,
        /// <summary>
        /// 已开启
        /// </summary>
        Opened = 1
    }
}
