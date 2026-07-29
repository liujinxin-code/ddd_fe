using Application.Abstractions;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions.Repositories
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

        /// <summary>
        /// 分页查询指定代理的下级用户（IsAgent=0 且未删除），支持排序字段与方向。
        /// 返回当前页数据与符合条件的总记录数。
        /// </summary>
        /// <param name="agentUserid">上级代理id</param>
        /// <param name="pageIndex">页码（从 1 开始）</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="sortField">排序字段（白名单：userid/username/email/useramount/userstatus/agentuserid/createby）</param>
        /// <param name="sortDesc">是否倒序</param>
        /// <param name="keyword">可选关键词，按用户名或邮箱模糊匹配（null/空表示查全部）</param>
        /// <param name="ct"></param>
        Task<(IReadOnlyList<TkUser> Items, int Total)> GetChildrenByAgentAsync(long agentUserid, int pageIndex, int pageSize, string? keyword, string? sortField, bool sortDesc, CancellationToken ct = default);
    }
}
