using Application.Abstractions.Caching;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Common.Caching
{
    public class CacheService<Entity> : ICacheService<Entity>
    {
        private readonly IDatabase _cacheDb;
        public string _cachePre { get; } = typeof(Entity).Name;
        public CacheService(ConnectionMultiplexer connection)
        {
            _cacheDb = connection?.GetDatabase() ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <summary>
        /// key是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(string key) => await _cacheDb.KeyExistsAsync($"{_cachePre}:{key.ToLower()}").ConfigureAwait(false);
        /// <summary>
        /// 获取redis
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _cacheDb.StringGetAsync($"{_cachePre}:{key.ToLower()}");
            if (value.IsNullOrEmpty)
                return default!;
            return JsonSerializer.Deserialize<T>(value!);
        }
        /// <summary>
        /// 根据key移除
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> RemoveAsync(string key) => await _cacheDb.KeyDeleteAsync($"{_cachePre}:{key.ToLower()}").ConfigureAwait(false);
        /// <summary>
        /// 存入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(value);
            key = $"{_cachePre}:{key.ToLower()}";
            return await _cacheDb.StringSetAsync(key, json, expiry, When.Always).ConfigureAwait(false);
        }

        /// <summary>
        /// set 添加
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<bool> SetMembersAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(value);
            key = $"{_cachePre}:{key.ToLower()}";
            var added = await _cacheDb.SetAddAsync(key, json);
            if (expiry.HasValue)
            {
                await _cacheDb.KeyExpireAsync(key, expiry);
            }
            return added;
        }
        /// <summary>
        /// set 获取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<IList<T>> GetMembersAsync<T>(string key)
        {
            key = $"{_cachePre}:{key.ToLower()}";
            var values = await _cacheDb.SetMembersAsync(key);
            return values
          .Select(v => JsonSerializer.Deserialize<T>(v!))
          .Where(x => x != null)
          .ToList()!;
        }

        /// <summary>
        /// 单个 set 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> RemoveSignleMembersAsync<T>(string key, T value)
        {
            key = $"{_cachePre}:{key.ToLower()}";
            string json = JsonSerializer.Serialize(value);
            return await _cacheDb.SetRemoveAsync(key, json);
        }

        /// <summary>
        ///  set key 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> RemoveKeyMembersAsync(string key)
        {
            key = $"{_cachePre}:{key.ToLower()}";
            return await _cacheDb.KeyDeleteAsync(key);
        }
    }
}
