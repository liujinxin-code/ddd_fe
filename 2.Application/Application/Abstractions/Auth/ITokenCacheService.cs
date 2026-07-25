using Application.Abstractions.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions.Auth
{
    public interface ITokenCacheService
    {
        /// <summary>
        /// 获取token是否存在
        /// </summary>
        /// <param name="jti"></param>
        /// <returns></returns>
        Task<bool> GetTokenExistsAsync(string jti);
        /// <summary>
        /// 登录，token存储和member添加
        /// </summary>
        /// <param name="jti"></param>
        /// <param name="userid"></param>
        /// <param name="signleClient">单客户端登录</param>
        /// <returns></returns>
        Task<bool> SetTokenAsync(string jti, long userid, int signleClient = 0);
        /// <summary>
        /// 退出登录，删除token 和member
        /// </summary>
        /// <param name="jti"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        Task<bool> RemoveTokenAsync(string jti, long userid);
    }
}
