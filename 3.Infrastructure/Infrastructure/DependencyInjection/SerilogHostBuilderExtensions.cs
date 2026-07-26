using Infrastructure.Common.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

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
                loggerConfiguration
                    .MinimumLevel.Verbose()
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.ClassifiedFile(AppContext.BaseDirectory);
            });
        }
    }
}
