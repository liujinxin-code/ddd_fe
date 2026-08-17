namespace Infrastructure.Common.RateLimit
{
    /// <summary>
    /// 滑动限流配置。所有阈值均可在 appsettings 的 "RateLimit" 段或
    /// 环境变量 RateLimit__* 覆盖（docker-compose 中已注入）。
    /// </summary>
    public class RateLimitOptions
    {
        public const string SectionName = "RateLimit";

        /// <summary>需要授权的接口：单个 (userid:jti) 在窗口内的允许请求数。默认 200。</summary>
        public int AuthorizedLimit { get; set; } = 200;

        /// <summary>需要授权的接口限流窗口（秒）。默认 30。</summary>
        public int AuthorizedWindowSeconds { get; set; } = 30;

        /// <summary>匿名接口：单个客户端 IP 在窗口内的允许请求数。默认 60。</summary>
        public int AnonymousLimit { get; set; } = 60;

        /// <summary>匿名接口限流窗口（秒）。默认 30。</summary>
        public int AnonymousWindowSeconds { get; set; } = 30;

        /// <summary>是否对匿名接口启用限流（默认 true）。设为 false 可整体关闭匿名限流。</summary>
        public bool EnableAnonymous { get; set; } = true;

        /// <summary>
        /// 受保护接口但 Token 缺失/无效时的兜底限流（按客户端 IP，防暴力破解/撞库）。
        /// 通常应比正常授权阈值更严格。默认 20。
        /// </summary>
        public int UnauthenticatedLimit { get; set; } = 20;

        /// <summary>兜底限流窗口（秒）。默认 30。</summary>
        public int UnauthenticatedWindowSeconds { get; set; } = 30;
    }
}
