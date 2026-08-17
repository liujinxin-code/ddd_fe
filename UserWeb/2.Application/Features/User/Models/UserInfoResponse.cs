namespace Application.Features.User.Models
{
    public class UserInfoResponse
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public long Userid { get; set; }

        /// <summary>
        /// 用户邮箱账号
        /// </summary>

        public string Email { get; private set; } = default!;
        /// <summary>
        /// 用户名名
        /// </summary>
        public string Username { get; set; } = default!;

        /// <summary>
        /// 用户余额
        /// </summary>
        public decimal UserAmount { get; private set; }

        /// <summary>
        /// 上级代理id 0则没有上级代理
        /// </summary>
        public long AgentUserid { get; private set; }
        /// <summary>
        /// 是否为代理
        /// </summary>
        public int IsAgent { get; private set; }
        /// <summary>
        /// 代理收益余额
        /// </summary>
        public decimal AgentAmount { get; private set; }
        /// <summary>
        /// 代理域名
        /// </summary>
        public string AgentDomain { get; private set; } = default!;

    }
}
