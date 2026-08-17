using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.IO;

namespace Infrastructure.DependencyInjection
{
    /// <summary>
    /// 配置 Serilog：
    /// 1) 控制台输出全部级别；
    /// 2) 文件按 Logger&lt;T&gt; 的 T（SourceContext 简名）分文件，仅记录 Warning/Error；
    /// 3) 使用 Serilog 原生能力：按天滚动（rollingInterval）+ 50MB 大小滚动（fileSizeLimitBytes/rollOnFileSizeLimit）
    ///    + retainedFileTimeLimit 7 天自动删除。
    /// 日志落在 AppContext.BaseDirectory（即 .../bin/Debug/net8.0/）目录下。
    /// </summary>
    public static class SerilogHostBuilderExtensions
    {
        public static IHostBuilder UseInfrastructureSerilog(
            this IHostBuilder hostBuilder,
            IConfiguration configuration)
        {
            return hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
            {
                // AppContext.BaseDirectory = .../bin/Debug/net8.0/，日志即落在 bin 目录树下，如 bin/Debug/net8.0/Level/UserController20260726.log
                var logRoot = Path.Combine(AppContext.BaseDirectory);
                var retentionDays = int.TryParse(configuration["Logging:RetentionDays"], out var rd) ? rd : 7;

                loggerConfiguration
                    .MinimumLevel.Verbose()
                    .Enrich.FromLogContext()
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.Map(
                        evt => GetClassName(evt),
                        (className, wt) => wt.File(
                            path: Path.Combine(logRoot, "logs", className.Item1, $"{className.Item2}.log"),
                            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}",
                            restrictedToMinimumLevel: LogEventLevel.Warning,
                            rollingInterval: RollingInterval.Day,
                            fileSizeLimitBytes: 50L * 1024 * 1024,
                            rollOnFileSizeLimit: true,
                            retainedFileTimeLimit: TimeSpan.FromDays(retentionDays),
                            retainedFileCountLimit: null),
                        restrictedToMinimumLevel: LogEventLevel.Warning);
            });
        }

        /// <summary>
        /// 取 SourceContext 的简名作为文件名（如 Open.Controllers.UserController -> UserController；泛型去掉 ` 后缀）。
        /// 无 SourceContext 时统一写入 Unknown.log。
        /// </summary>
        private static (string, string) GetClassName(LogEvent logEvent)
        {
            if (logEvent.Properties.TryGetValue("SourceContext", out var sc)
                && sc is ScalarValue { Value: string s }
                && !string.IsNullOrWhiteSpace(s))
            {
                var simple = s.Split('.').Last();
                var tick = simple.IndexOf('`');
                if (tick > 0)
                {
                    simple = simple[..tick];
                }

                return ($"{logEvent.Level}".ToLower(), simple);
            }

            return ($"{logEvent.Level}".ToLower(), "Unknown");
        }
    }
}
