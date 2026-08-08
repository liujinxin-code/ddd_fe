using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Common.RateLimit
{
    /// <summary>
    /// 基于 Redis 有序集合（zset）的真·滑动窗口限流实现。
    ///
    /// 算法（滑动窗口日志）：每个请求以"当前 Unix 毫秒时间戳"为 score 写入一个唯一成员，
    /// 先剔除窗口外的旧成员，再统计当前窗口内的成员数；整个判定在一段 Lua 脚本内
    /// 原子完成（仅一次 RTT，避免并发竞态导致的计数误差）。
    ///
    /// 依赖已在 InfrastructureServiceCollectionExtensions 中注册的 ConnectionMultiplexer 单例。
    /// </summary>
    public class SlidingWindowRateLimiter : ISlidingWindowRateLimiter
    {
        // KEYS[1] = 限流键
        // ARGV[1] = 当前毫秒时间戳
        // ARGV[2] = 窗口大小（毫秒）
        // ARGV[3] = 阈值
        // ARGV[4] = 本次请求的唯一成员（now-guid）
        // 返回 { 当前计数, 窗口重置时刻(毫秒) }
        private static readonly string Script = @"
local key = KEYS[1]
local now = tonumber(ARGV[1])
local window = tonumber(ARGV[2])
local limit = tonumber(ARGV[3])
local member = ARGV[4]
local clearBefore = now - window

redis.call('ZREMRANGEBYSCORE', key, 0, clearBefore)
redis.call('ZADD', key, now, member)
local count = redis.call('ZCARD', key)
redis.call('EXPIRE', key, math.ceil(window / 1000) + 2)

local oldest = redis.call('ZRANGE', key, 0, 0, 'WITHSCORES')
local resetAt = now + window
if #oldest >= 2 then
  resetAt = tonumber(oldest[2]) + window
end
return { count, resetAt }
";

        private readonly IDatabase _db;

        public SlidingWindowRateLimiter(ConnectionMultiplexer multiplexer)
        {
            _db = (multiplexer ?? throw new ArgumentNullException(nameof(multiplexer))).GetDatabase();
        }

        public async Task<RateLimitResult> CheckAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken = default)
        {
            var nowMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var windowMs = (long)window.TotalMilliseconds;
            var member = $"{nowMs}-{Guid.NewGuid():N}";

            var result = await _db.ScriptEvaluateAsync(
                Script,
                new RedisKey[] { key },
                new RedisValue[] { nowMs, windowMs, limit, member }).ConfigureAwait(false);

            var arr = (RedisResult[]?)result;
            if (arr is null || arr.Length < 2)
            {
                // 脚本未按预期返回（理论上不会发生），fail-open 放行，避免拖垮正常业务
                return new RateLimitResult
                {
                    IsAllowed = true,
                    Count = 0,
                    Limit = limit,
                    ResetAtUnixMs = nowMs + windowMs,
                    ResetAtUnixSeconds = (nowMs + windowMs) / 1000
                };
            }

            var count = (int)(long)arr[0];
            var resetAtMs = (long)arr[1];

            return new RateLimitResult
            {
                IsAllowed = count <= limit,
                Count = count,
                Limit = limit,
                ResetAtUnixMs = resetAtMs,
                ResetAtUnixSeconds = resetAtMs / 1000,
                RetryAfterSeconds = count <= limit ? 0 : (int)Math.Ceiling((resetAtMs - nowMs) / 1000.0)
            };
        }
    }
}
