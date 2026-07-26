using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace Infrastructure.Common.Logging
{
    internal static class ClassifiedFileSinkExtensions
    {
        public static LoggerConfiguration ClassifiedFile(
            this LoggerSinkConfiguration sinkConfiguration,
            string applicationRootPath,
            LogEventLevel restrictedToMinimumLevel = LogEventLevel.Warning,
            int retentionDays = 7)
        {
            return sinkConfiguration.Sink(
                new ClassifiedFileSink(applicationRootPath, null, retentionDays),
                restrictedToMinimumLevel);
        }
    }
}
