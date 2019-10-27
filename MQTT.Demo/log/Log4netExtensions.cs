using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MQTT.Demo.log
{
    public static class Log4netExtensions
    {
        public static ILoggingBuilder AddLog4Net(this ILoggingBuilder builder, bool skipDiagnosticLogs)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config");
            builder.Services.AddSingleton((ILoggerProvider) new Log4NetProvider(path, skipDiagnosticLogs));
            return builder;
        }
    }
}