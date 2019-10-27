using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTT.Demo.log;

namespace MQTT.Demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new HostBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddEnvironmentVariables();
                    config.AddJsonFile("appsettings.json", true, false);
                })
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddLog4Net(true);
                    builder.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    //    添加依赖注入
                    DiBuilder.Build(services, hostContext.Configuration);
                    //    添加通用主机启动HistedService
                    services.AddHostedService<MqttHostedService>();
                })
                .RunConsoleAsync();
        }
    }
}