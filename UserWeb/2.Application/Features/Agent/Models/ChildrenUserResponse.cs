using Domain.Enums;
using System;

namespace Application.Features.Agent.Models
{
    /// <summary>
    /// 下级用户列表项。仅暴露展示所需字段，不含密码/ApiKey 等敏感信息。
    /// </summary>
    public class ChildrenUserResponse
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public long Userid { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 余额
        /// </summary>
        public decimal UserAmount { get; set; }

        /// <summary>
        /// 用户状态
        /// </summary>
        public TkUserStatus UserStatus { get; set; }

        /// <summary>
        /// 上级代理id（即当前代理）
        /// </summary>
        public long AgentUserid { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string Createby { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreateTime { get; set; }
    }
}
