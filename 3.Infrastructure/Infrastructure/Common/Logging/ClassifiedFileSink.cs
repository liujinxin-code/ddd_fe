using Serilog.Core;
using Serilog.Events;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace Infrastructure.Common.Logging
{
    /// <summary>
    /// Writes warning and error logs to separate files by ILogger&lt;T&gt; class name.
    /// </summary>
    internal sealed class ClassifiedFileSink : ILogEventSink
    {
        private const string SourceContextPropertyName = "SourceContext";
        private static readonly ConcurrentDictionary<string, object> FileLocks = new();
        private readonly IFormatProvider? _formatProvider;
        private readonly string _logRootPath;

        public ClassifiedFileSink(string applicationRootPath, IFormatProvider? formatProvider = null)
        {
            _formatProvider = formatProvider;
            _logRootPath = Path.Combine(applicationRootPath);
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent.Level < LogEventLevel.Warning)
            {
                return;
            }

            var className = GetClassName(logEvent);
            var directory = Path.Combine(_logRootPath, $"{logEvent.Level}".ToLower(), className);
            Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, $"{logEvent.Timestamp:yyyyMMdd}.log");
            var fileLock = FileLocks.GetOrAdd(filePath, _ => new object());

            lock (fileLock)
            {
                File.AppendAllText(filePath, Format(logEvent), Encoding.UTF8);
            }
        }

        private string Format(LogEvent logEvent)
        {
            var builder = new StringBuilder();
            builder.Append(CultureInfo.InvariantCulture, $"{logEvent.Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} ");
            builder.Append('[').Append(logEvent.Level).Append("] ");
            builder.Append(logEvent.RenderMessage(_formatProvider));
            builder.AppendLine();

            if (logEvent.Exception is not null)
            {
                builder.AppendLine(logEvent.Exception.ToString());
            }

            return builder.ToString();
        }

        private static string GetClassName(LogEvent logEvent)
        {
            if (logEvent.Properties.TryGetValue(SourceContextPropertyName, out var sourceContext)
                && sourceContext is ScalarValue { Value: string value }
                && !string.IsNullOrWhiteSpace(value))
            {
                var simpleName = value.Split('.').Last();
                var genericMarkerIndex = simpleName.IndexOf('`');

                if (genericMarkerIndex > 0)
                {
                    simpleName = simpleName[..genericMarkerIndex];
                }

                return SanitizePathName(simpleName);
            }

            return "Unknown";
        }

        private static string SanitizePathName(string value)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var builder = new StringBuilder(value.Length);

            foreach (var character in value)
            {
                builder.Append(invalidChars.Contains(character) ? '_' : character);
            }

            return builder.ToString();
        }
    }
}
