using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Abstractions.Caching
{
    /// <summary>
    /// 缓存接口服务
    /// </summary>
    public interface ICacheService<Entity>
    {

        string _cachePre { get; }

        /// <summary>
        /// 获取指定 key 的值并反序列化为 T 类型
        /// </summary>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// 设置 key-value，可指定过期时间
        /// </summary>
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);

        /// <summary>
        /// 删除指定 key
        /// </summary>
        Task<bool> RemoveAsync(string key);

        /// <summary>
        /// 判断 key 是否存在
        /// </summary>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// set 添加
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task<bool> SetMembersAsync<T>(string key, T value, TimeSpan? expiry = null);
        /// <summary>
        /// set 获取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<IList<T>> GetMembersAsync<T>(string key);

        /// <summary>
        /// 单个 set 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<bool> RemoveSignleMembersAsync<T>(string key, T value);

        /// <summary>
        ///  set key 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<bool> RemoveKeyMembersAsync<T>(string key);
    }
}
