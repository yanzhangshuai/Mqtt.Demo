using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTT.Demo.Core.Kafka.Consumer;
using MQTT.Demo.Services.Mqtt;

namespace MQTT.Demo.Services.Kafka
{
    public class KafkaWriteDataConsumerManager:KafkaConsumerManagerBase<string,KafkaWriteDataInput>
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Func<string, IMqttService> _mqttServiceFunc;

        public KafkaWriteDataConsumerManager(ILogger<KafkaWriteDataConsumerManager> logger, Func<string,
            IMqttService> mqttServiceFunc) : base(logger)
        {
            _mqttServiceFunc = mqttServiceFunc;
            _cancellationTokenSource = new CancellationTokenSource();
            _token = _cancellationTokenSource.Token;
        }
        public override void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        protected override async Task Consume(string key, KafkaWriteDataInput value, DateTime utcMessageTimeStamp)
        {
            //    根据其MQttId进行调用具体的逻辑处理代码,使得每个mqtt的代码都集中在一起,方便维护
            await _mqttServiceFunc(value.MqttName).WriteDataAsync(value);
        }
    }
}