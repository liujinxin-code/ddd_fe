namespace Infrastructure.Common.RateLimit
{
    /// <summary>
    /// 单次滑动窗口限流判定结果。
    /// </summary>
    public class RateLimitResult
    {
        /// <summary>是否允许本次请求。</summary>
        public bool IsAllowed { get; set; }

        /// <summary>当前窗口内已计数（含本次）。</summary>
        public int Count { get; set; }

        /// <summary>本次使用的阈值。</summary>
        public int Limit { get; set; }

        /// <summary>窗口重置时刻（Unix 毫秒，UTC）。</summary>
        public long ResetAtUnixMs { get; set; }

        /// <summary>窗口重置时刻（Unix 秒，UTC），用于 X-RateLimit-Reset 响应头。</summary>
        public long ResetAtUnixSeconds { get; set; }

        /// <summary>距窗口重置还需等待的秒数（被限流时用于 Retry-After 响应头）。</summary>
        public int RetryAfterSeconds { get; set; }
    }
}
