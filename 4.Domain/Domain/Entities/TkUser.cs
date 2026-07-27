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

        public void RequiredUserStatus()
        {
            if (UserStatus != TkUserStatus.Enable)
            {
                throw new InvalidOperationException("用户已禁用！");
            }
        }
        /// <summary>
        /// 校验是否为下级用户
        /// </summary>
        /// <param name="agentUserid"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void RequiredChildFunc(long agentUserid)
        {
            if (AgentUserid != agentUserid || IsAgentFnc())
            {
                throw new InvalidOperationException("用户归属不一致！");
            }
        }
        /// <summary>
        /// 校验是否为代理
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void RequiredAgentFunc()
        {
            if (!IsAgentFnc())
            {
                throw new InvalidOperationException("用户非代理！");
            }
        }
        /// <summary>
        /// 代理转增余额到下级用户，扣减的UserAmount
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="chilren"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void TransferAmountToChildrenFunc(decimal amount, TkUser chilren)
        {
            if (UserAmount <= 0 || UserAmount - amount < 0)
            {
                throw new InvalidOperationException("余额不足，无法赠送！");
            }

            RequiredAgentFunc();
            chilren.RequiredChildFunc(Userid);
            UserAmount -= amount;
            chilren.UserAmount += amount;
            Touch();
            chilren.Touch();

        }

        /// <summary>
        /// 重置下级用户密码
        /// </summary>
        /// <param name="children"></param>
        public void ResetChildrenPasswordFunc(TkUser children, string newPassword)
        {
            RequiredAgentFunc();
            children.RequiredChildFunc(Userid);
            children.Password = newPassword;
            children.Touch();
        }

        /// <summary>
        /// 修改下级用户状态
        /// </summary>
        /// <param name="children"></param>
        public void UpdateChildrenStatusFunc(TkUser children, TkUserStatus tkUserStatus)
        {
            RequiredAgentFunc();
            children.RequiredChildFunc(Userid);
            children.UserStatus = tkUserStatus;
            children.Touch();
        }
        /// <summary>
        /// 每次修改用户核心信息时递增版本号。
        /// 后续如果需要乐观锁或缓存版本校验，可以复用该字段。
        /// </summary>
        private void Touch()
        {
            UserVersion++;
        }
    }
}
