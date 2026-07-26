using Application.Abstractions;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Events.User.Contracts
{
    public interface ITkUserRepository : IRepository<TkUser>
    {

        /// <summary>
        /// 通过代理域名获取代理
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TkUser?> GetAgentByDomain(string agentDomain, CancellationToken ct = default);

        /// <summary>
        /// 查询邮箱是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> GetEmailExists(string email, CancellationToken ct = default);

        /// <summary>
        /// 查询用户名是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> GetUserNameExists(string username, CancellationToken ct = default);

        /// <summary>
        /// 根据邮箱或用户名获取用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TkUser?> GetUserByUserNameOrEmailAsync(string name, CancellationToken ct = default);

        /// <summary>
        /// 判断用户id和ApiKey是否存在
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="apiKey"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<bool> GetUserExistsByApiKey(long userid, string apiKey, CancellationToken ct = default);
    }
}
