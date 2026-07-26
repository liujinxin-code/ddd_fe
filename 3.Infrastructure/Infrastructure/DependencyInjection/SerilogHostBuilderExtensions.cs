using Infrastructure.Common.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.IO;

namespace Infrastructure.DependencyInjection
{
    public static class SerilogHostBuilderExtensions
    {
        public static IHostBuilder UseInfrastructureSerilog(
            this IHostBuilder hostBuilder,
            IConfiguration configuration)
        {
            return hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
            {
                // AppContext.BaseDirectory = .../bin/Debug/net8.0/，日志即落在 bin 目录树下（bin/Debug/net8.0/error|warning/类名/日期.log）
                var logRoot = Path.Combine(AppContext.BaseDirectory);
                var retentionDays = int.TryParse(configuration["Logging:RetentionDays"], out var rd) ? rd : 7;

                loggerConfiguration
                    .MinimumLevel.Verbose()
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.ClassifiedFile(logRoot, retentionDays: retentionDays);
            });
        }
    }
}
