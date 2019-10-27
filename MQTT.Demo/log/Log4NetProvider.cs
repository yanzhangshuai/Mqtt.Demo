using System.Collections.Concurrent;
using System.IO;
using Microsoft.Extensions.Logging;

namespace MQTT.Demo.log
{
    public class Log4NetProvider : ILoggerProvider
    {
        private readonly string _log4NetConfigFile;

        private readonly ConcurrentDictionary<string, ILogger> _loggers
            = new ConcurrentDictionary<string, ILogger>();

        private readonly bool _skipDiagnosticLogs;

        public Log4NetProvider(string log4NetConfigFile, bool skipDiagnosticLogs)
        {
            _log4NetConfigFile = log4NetConfigFile;
            _skipDiagnosticLogs = skipDiagnosticLogs;
        }

        public void Dispose()
        {
            _loggers.Clear();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName,
                name => new Log4NetLogger(name, new FileInfo(_log4NetConfigFile), _skipDiagnosticLogs));
        }
    }
}