using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Open.Common;
using Shared.Utilities;

namespace Open.Middlewares;

/// <summary>
/// 请求时区中间件：解析 <c>X-TimeZone</c> 请求头（IANA 时区 ID，如 Asia/Shanghai / America/New_York），
/// 将时区 ID 写入 <see cref="TimeZoneContext"/>，供全局 DateTimeOffset 序列化转换器把上海存储时间换算成该时区。
///
/// 解析优先级：请求头 X-TimeZone → 配置 TimeZone:Default（默认 Asia/Shanghai）→ 非法/未知 ID 兜底 UTC。
/// 时区解析统一走 <see cref="TimeZoneHelper.GetZoneSafe"/>（唯一入口，带缓存与 UTC 兜底）。
/// 数据库统一以上海时区存储（DateTimeOffset.Now），仅在 HTTP 出口按请求时区转换；ORM / Service / Controller 全程无感知。
/// </summary>
public sealed class RequestTimeZoneMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _defaultTimeZoneId;

    public RequestTimeZoneMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        // 默认 Asia/Shanghai：面向国内用户，前端忘传头时也不会差 8 小时。
        _defaultTimeZoneId = configuration["TimeZone:Default"] ?? "Asia/Shanghai";
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tzId = context.Request.Headers["X-TimeZone"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(tzId))
        {
            tzId = _defaultTimeZoneId;
        }

        // 解析失败 / 缺 tzdata 时，GetZoneSafe 内部兜底 UTC，请求不挂。
        TimeZoneContext.Current.Value = tzId;

        try
        {
            await _next(context);
        }
        finally
        {
            // 清理，防止 AsyncLocal 跨请求泄漏（连接池 / 异步流复用场景）。
            TimeZoneContext.Current.Value = null;
        }
    }
}
