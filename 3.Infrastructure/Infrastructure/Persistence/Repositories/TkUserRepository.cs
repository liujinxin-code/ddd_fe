using Application.Abstractions;
using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class TkUserRepository(AppDbContext appDbContext) : IRepository<TkUser>, ITkUserRepository
    {
        public async Task<bool> AddAsync(TkUser entity, CancellationToken ct = default)
        {
            return await appDbContext.TkUsers.AddAsync(entity, ct) != null;
        }
        public async Task<TkUser?> GetByEmailAsync(string email)
        {
            return await appDbContext.TkUsers.AsNoTracking().FirstOrDefaultAsync(t => t.Email == email);
        }
        public async Task<TkUser?> GetByIdAsync(long id, CancellationToken ct = default)
        {
            return await appDbContext.TkUsers.FirstOrDefaultAsync(t => t.Userid == id && !t.IsDelete, ct);
        }
        /// <summary>
        /// 通过代理域名获取代理
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TkUser?> GetAgentByDomain(string agentDomain, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(agentDomain))
            {
                return default!;
            }
            agentDomain = agentDomain.Trim().ToLower();
            return await appDbContext.TkUsers.AsNoTracking().FirstOrDefaultAsync(t => t.IsAgent == 1 && t.AgentDomain == agentDomain, ct);
        }


        /// <summary>
        /// 查询邮箱是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> GetEmailExists(string email, CancellationToken ct = default)
        {

            return await appDbContext.TkUsers.AsNoTracking().CountAsync(t => t.Email == email, ct) > 0;
        }

        /// <summary>
        /// 查询用户名是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> GetUserNameExists(string username, CancellationToken ct = default)
        {

            return await appDbContext.TkUsers.AsNoTracking().CountAsync(t => t.Username == username, ct) > 0;
        }
        /// <summary>
        /// 判断用户id和ApiKey是否存在
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="apiKey"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<bool> GetUserExistsByApiKey(long userid, string apiKey, CancellationToken ct = default)
        {
            return await appDbContext.TkUsers.AsNoTracking().CountAsync(t => t.Userid == userid && t.ApiKey == apiKey && t.UserStatus == Domain.Enums.TkUserStatus.Enable && !t.IsDelete, ct) > 0;
        }

        public void Update(TkUser entity, CancellationToken ct = default)
        {
            appDbContext.TkUsers.Update(entity);
        }

        public async Task<TkUser?> GetUserByUserNameOrEmailAsync(string name, CancellationToken ct = default)
        {
            var user = await appDbContext.TkUsers.AsNoTracking().FirstOrDefaultAsync(t => (t.Username == name || t.Email == name) && !t.IsDelete, ct);
            return user;
        }

        /// <summary>
        /// 分页查询指定代理的下级用户（IsAgent=0 且未删除），支持排序字段与方向。
        /// </summary>
        public async Task<(IReadOnlyList<TkUser> Items, int Total)> GetChildrenByAgentAsync(long agentUserid, int pageIndex, int pageSize, string? keyword, string? sortField, bool sortDesc, int? userStatus = null, CancellationToken ct = default)
        {
            var query = appDbContext.TkUsers.AsNoTracking()
                .Where(t => t.AgentUserid == agentUserid && t.IsAgent == 0 && !t.IsDelete);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(t => t.Username.Contains(keyword) || t.Email.Contains(keyword));
            }

            if (userStatus.HasValue)
            {
                query = query.Where(t => t.UserStatus == (Domain.Enums.TkUserStatus)userStatus.Value);
            }

            int total = await query.CountAsync(ct);

            query = ApplyChildrenSorting(query, sortField, sortDesc);

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        /// <summary>
        /// 统计指定代理的下级用户数量：已启用数与总数（IsAgent=0 且未删除）。
        /// </summary>
        public async Task<(int EnabledCount, int TotalCount)> GetChildrenStatsAsync(long agentUserid, CancellationToken ct = default)
        {
            var query = appDbContext.TkUsers.AsNoTracking()
                .Where(t => t.AgentUserid == agentUserid && t.IsAgent == 0 && !t.IsDelete);

            int total = await query.CountAsync(ct);
            int enabled = await query.CountAsync(t => t.UserStatus == Domain.Enums.TkUserStatus.Enable, ct);

            return (enabled, total);
        }

        /// <summary>
        /// 仅允许白名单内的字段排序，避免任意列名导致 EF 翻译失败或被用于注入。
        /// </summary>
        private static IQueryable<TkUser> ApplyChildrenSorting(IQueryable<TkUser> query, string? sortField, bool sortDesc)
        {
            var field = (sortField ?? string.Empty).Trim().ToLowerInvariant();
            Expression<Func<TkUser, object>> keySelector = field switch
            {
                "username" => t => t.Username,
                "email" => t => t.Email,
                "useramount" => t => t.UserAmount,
                "userstatus" => t => t.UserStatus,
                "agentuserid" => t => t.AgentUserid,
                "createby" => t => t.Createby,
                "createtime" => t => t.CreateTime,
                "userid" => t => t.Userid,
                _ => t => t.Userid
            };

            return sortDesc ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        public Task<TkUser?> GetByIdAsNoTrackingAsync(long id, CancellationToken ct = default)
           => appDbContext.TkUsers.AsNoTracking().Where(t => t.Userid == id).FirstOrDefaultAsync();
    }
}
