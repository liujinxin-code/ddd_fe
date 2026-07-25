using Application.Abstractions;
using Application.User.Contracts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
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
        public Task<TkUser?> GetByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }
        public async Task<TkUser?> GetByIdAsync(long id, CancellationToken ct = default)
        {
            return await appDbContext.TkUsers.AsNoTracking().FirstOrDefaultAsync(t => t.Userid == id, ct);
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
            return await appDbContext.TkUsers.AsNoTracking().CountAsync(t => t.Userid == userid && t.ApiKey == apiKey, ct) > 0;
        }

        public void Update(TkUser entity, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<TkUser?> GetUserByUserNameOrEmailAsync(string name, CancellationToken ct = default)
        {
            var user = await appDbContext.TkUsers.AsNoTracking().FirstOrDefaultAsync(t => t.Username == name || t.Email == name, ct);
            return user;
        }
    }
}
