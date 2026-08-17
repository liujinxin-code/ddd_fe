using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Common.RateLimit
{
    /// <summary>
    /// 基于 Redis 的滑动窗口限流器。
    /// </summary>
    public interface ISlidingWindowRateLimiter
    {
        /// <summary>
        /// 判定指定 key 在当前窗口内是否超限。
        /// </summary>
        /// <param name="key">限流键（如 ratelimit:auth:{userid}:{jti}）。</param>
        /// <param name="limit">窗口内允许的最大请求数。</param>
        /// <param name="window">滑动窗口大小。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>本次判定结果。</returns>
        Task<RateLimitResult> CheckAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken = default);
    }
}
