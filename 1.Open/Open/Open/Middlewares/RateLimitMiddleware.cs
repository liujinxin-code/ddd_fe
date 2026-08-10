using Application.Common.Models;
using Infrastructure.Common.RateLimit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Open.Middlewares
{
    /// <summary>
    /// 滑动窗口限流中间件。
    ///
    /// 规则：
    ///  - 仅对 /api 前缀的接口生效，跳过 CORS 预检（OPTIONS）。
    ///  - 匿名接口（端点元数据含 IAllowAnonymous）：按客户端 IP 限流。
    ///  - 需要授权的接口且身份有效：key = ratelimit:auth:{userid}:{jti}（按令牌维度）。
    ///  - 需要授权的接口但 Token 缺失/无效：按客户端 IP 兜底限流，防暴力破解。
    ///
    /// 限流判定依赖 Redis；若 Redis 异常则 fail-open（放行），避免限流组件拖垮正常业务。
    /// </summary>
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISlidingWindowRateLimiter _limiter;
        private readonly RateLimitOptions _options;
        private readonly ILogger<RateLimitMiddleware> _logger;

        public RateLimitMiddleware(
            RequestDelegate next,
            ISlidingWindowRateLimiter limiter,
            IOptions<RateLimitOptions> options,
            ILogger<RateLimitMiddleware> logger)
        {
            _next = next;
            _limiter = limiter;
            _options = options.Value;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 只对 /api 接口限流；跳过 CORS 预检等
            if (!context.Request.Path.StartsWithSegments("/api") ||
                HttpMethods.IsOptions(context.Request.Method))
            {
                await _next(context);
                return;
            }

            var endpoint = context.GetEndpoint();
            var isAnonymous = endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null;

            string key;
            int limit;
            int windowSeconds;

            if (isAnonymous)
            {
                if (!_options.EnableAnonymous)
                {
                    await _next(context);
                    return;
                }

                key = $"ratelimit:anon:ip:{GetClientIp(context)}";
                limit = _options.AnonymousLimit;
                windowSeconds = _options.AnonymousWindowSeconds;
            }
            else
            {
                var currentUser = context.RequestServices.GetRequiredService<ICurrentUser>();
                if (currentUser.IsAuthenticated && currentUser.Userid > 0 && !string.IsNullOrEmpty(currentUser.Jti))
                {
                    key = $"ratelimit:auth:{currentUser.Userid}:{currentUser.Jti}";
                    limit = _options.AuthorizedLimit;
                    windowSeconds = _options.AuthorizedWindowSeconds;
                }
                else
                {
                    // 受保护接口但无有效身份：按 IP 兜底，防暴力破解
                    key = $"ratelimit:authfail:ip:{GetClientIp(context)}";
                    limit = _options.UnauthenticatedLimit;
                    windowSeconds = _options.UnauthenticatedWindowSeconds;
                }
            }

            RateLimitResult? result = null;
            try
            {
                result = await _limiter.CheckAsync(key, limit, TimeSpan.FromSeconds(windowSeconds), context.RequestAborted);
            }
            catch (Exception ex)
            {
                // Redis 异常时 fail-open：放行请求，避免限流组件拖垮正常业务
                _logger.LogWarning(ex, "滑动限流校验失败，已放行请求 key={Key}", key);
            }

            if (result != null && !result.IsAllowed)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["Retry-After"] = result.RetryAfterSeconds.ToString();
                context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = "0";
                context.Response.Headers["X-RateLimit-Reset"] = result.ResetAtUnixSeconds.ToString();
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new ApiResult { Code = 429, Message = "请求过于频繁，请稍后再试" });
                return;
            }

            if (result != null)
            {
                context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, limit - result.Count).ToString();
            }

            await _next(context);
        }

        /// <summary>
        /// 解析客户端真实 IP。优先取反向代理透传的 X-Forwarded-For / X-Real-IP，
        /// 否则取直接对端 IP。注意：经反向代理时必须由代理正确设置上述头，否则所有请求会归并到代理 IP。
        /// </summary>
        private static string GetClientIp(HttpContext context)
        {
            var xff = context.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrWhiteSpace(xff))
            {
                var first = xff.Split(',')[0].Trim();
                if (IPAddress.TryParse(first, out var ip))
                {
                    return ip.ToString();
                }
            }

            var xri = context.Request.Headers["X-Real-IP"].ToString();
            if (!string.IsNullOrWhiteSpace(xri) && IPAddress.TryParse(xri, out var rip))
            {
                return rip.ToString();
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
