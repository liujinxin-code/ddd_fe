namespace Open.Common;

/// <summary>
/// 请求级时区上下文。通过 AsyncLocal 在单个请求的执行流内传递"当前请求时区"，
/// 由 <see cref="Open.Middlewares.RequestTimeZoneMiddleware"/> 在管道入口设置、出口（finally）清理。
/// 全局 DateTimeOffset 序列化转换器在写出阶段读取此处的值，将时间统一转换为"请求所指时区"的本地时间。
///
/// 为什么不用 DI Singleton：HTTP 请求天然并发，Singleton 会被所有请求共享导致串时区；
/// AsyncLocal 才是"按请求执行流隔离"的正确手段。
/// </summary>
public static class TimeZoneContext
{
    /// <summary>
    /// 当前请求的时区；未设置（或为未知时区兜底）时为 null，转换器据此按原值/UTC 输出。
    /// AsyncLocal 绑定在当前异步执行流，每次异步请求都是独立的一个变量
    /// </summary>
    public static readonly AsyncLocal<TimeZoneInfo?> Current = new();
}
