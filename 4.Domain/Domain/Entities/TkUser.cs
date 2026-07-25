using Domain.Auditors;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class TkUser : DeleteAuditor
    {

        public TkUser(string email, string username, string password, TkUserStatus userStatus, long agentUserid, int isAgent, string agentDomain, string apiKey, string createby)
        {
            this.Email = email;
            this.Username = username;
            this.Password = password;
            this.UserStatus = userStatus;
            this.AgentDomain = agentDomain;
            this.ApiKey = apiKey;
            this.Createby = createby;
            this.IsAgent = isAgent;
            this.AgentUserid = agentUserid;
        }
        /// <summary>
        /// 用户id
        /// </summary>
        public long Userid { get; private set; }
        /// <summary>
        /// 用户邮箱账号
        /// </summary>

        public string Email { get; private set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; private set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; private set; }
        /// <summary>
        /// 用户状态
        /// </summary>

        public TkUserStatus UserStatus { get; private set; }

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
        public string AgentDomain { get; private set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string Createby { get; private set; }
        /// <summary>
        /// 用户进行api访问持久key
        /// </summary>
        public string ApiKey { get; private set; }
        /// <summary>
        /// 用户信息版本号
        /// </summary>
        public int UserVersion { get; private set; }
        /// <summary>
        /// 是否单客户端登录
        /// </summary>
        public int SignleClient { get; }
        public bool IsAgentFnc()
        {
            return IsAgent == 1;
        }
    }
}
