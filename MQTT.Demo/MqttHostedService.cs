using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTT.Demo.Core;

namespace MQTT.Demo
{
    public class MqttHostedService : IHostedService, IDisposable
    {
        
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private ILogger<MqttHostedService> _logger;
        private IList<IStopable> _stopables;

        public MqttHostedService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await new ValueTask();
            var scope = _serviceScopeFactory.CreateScope();
            var startup = scope.ServiceProvider.GetRequiredService<Startup>();
            _logger = scope.ServiceProvider.GetService<ILogger<MqttHostedService>>();
            //    调用startup执行项目代码
            _stopables = startup.Start(scope);
            Console.WriteLine("started.........");
            _logger.LogInformation("started........");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("begin to stop");
            _logger.LogInformation("begin to stop ");
            _stopables.ToList().ForEach(s => s.Stop());
            Console.WriteLine("stopped.....");
            _logger.LogInformation("stopped....");
            await new ValueTask();
        }

        public void Dispose()
        {
        }
    }
}