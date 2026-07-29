using Application.Abstractions.Auth;
using Application.Abstractions.Caching;
using Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Common.Auth
{
    public class TokenCacheService(
        ICacheService<TokenCacheService> cacheService
        ) : ITokenCacheService
    {
        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="jti"></param>
        /// <returns></returns>
        public async Task<bool> GetTokenExistsAsync(string jti)
        {
            return await cacheService.ExistsAsync($"token:{jti}");
        }
        /// <summary>
        /// 登录，token存储和member添加
        /// </summary>
        /// <param name="jti"></param>
        /// <param name="userid"></param>
        /// <param name="signleClient">单客户端登录</param>
        /// <returns></returns>
        public async Task<bool> SetTokenAsync(string jti, long userid, int signleClient = 0)
        {
            var addToken = await cacheService.SetAsync<long>($"token:{jti}", userid, TimeSpan.FromDays(7));
            if (signleClient == 1)
            {
                //获取所有token
                var memberTokens = await cacheService.GetMembersAsync<string>($"tokens:{userid}");
                //删除member
                foreach (var memberToken in memberTokens)
                {
                    await RemoveTokenAndMemberAsync(memberToken, userid);
                }
            }
            var addMember = await cacheService.SetMembersAsync($"tokens:{userid}", jti, TimeSpan.FromDays(7.01));
            return addToken && addMember;

        }
        /// <summary>
        /// 退出登录，删除token 和member
        /// </summary>
        /// <param name="jti"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public async Task<bool> RemoveTokenAsync(string jti, long userid)
        {
            return await cacheService.RemoveAsync($"token:{jti}") && await cacheService.RemoveSignleMembersAsync($"tokens:{userid}", jti);
        }

        /// <summary>
        /// 用户禁用删除全部Token
        /// </summary>
        /// <param name="jti"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public async Task<bool> UserBlackRemoveTokensAsync(long userid)
        {
            //获取所有token
            var memberTokens = await cacheService.GetMembersAsync<string>($"tokens:{userid}");
            foreach (var memberToken in memberTokens)
            {
                await RemoveTokenAndMemberAsync(memberToken, userid);
            }
            return await cacheService.RemoveKeyMembersAsync($"tokens:{userid}");
        }
        /// <summary>
        /// 删除Token和对应的Member
        /// </summary>
        /// <param name="jti"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        private async Task<bool> RemoveTokenAndMemberAsync(string jti, long userid)
          //移除set集合的jti 和 删除jti token
          => await cacheService.RemoveSignleMembersAsync<string>($"tokens:{userid}", jti) && await cacheService.RemoveAsync($"token:{jti}");

    }
}
