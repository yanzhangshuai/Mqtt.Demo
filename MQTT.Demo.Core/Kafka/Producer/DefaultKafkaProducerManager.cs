using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using MQTT.Demo.Core.Kafka.Models;

namespace MQTT.Demo.Core.Kafka.Producer
{
    public class DefaultKafkaProducerManager : IKafkaProducerManager
    {
        private readonly KafkaSetting _kafkaSetting;
        private readonly ILogger<DefaultKafkaProducerManager> _logger;
        private Lazy<IProducer<string, string>> _producer;

        public DefaultKafkaProducerManager(
            KafkaSetting kafkaSetting,
            ILogger<DefaultKafkaProducerManager> logger
        )
        {
            _kafkaSetting =
                kafkaSetting ?? throw new ArgumentNullException(nameof(kafkaSetting));
            _logger = logger;
            Init();
        }

        public void Stop()
        {
            _producer.Value.Dispose();
        }

        public async Task PushDataAsync<T>(string topicName, string key, params T[] valueArr)
            where T : KafkaProducerSetting
        {
            //    以MQtt进行分区推送
            var valueGroups = valueArr.GroupBy(y => y.MqttName);
            foreach (var valueGroup in valueGroups)
            {
                var partitionIndex = Math.Abs(valueGroup.Key.GetHashCode()) %
                                     _kafkaSetting.TopicPartitionCount;
                foreach (var value in valueGroup)
                    _producer.Value.Produce(new TopicPartition(topicName, partitionIndex), new Message<string, string>
                    {
                        Key = key,
                        Value = JsonSerializer.Serialize(value)
                    });
            }

            await new ValueTask();
        }

        private void Init()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = _kafkaSetting.BootstrapServers,
                RequestTimeoutMs = 2000
            };
            _producer = new Lazy<IProducer<string, string>>(() =>
                new ProducerBuilder<string, string>(config)
                    .SetErrorHandler((p, err) =>
                    {
                        _logger.LogError($"producer error {err.Reason}{err.Code} {err.IsFatal}");
                    }).Build()
            );
        }
    }
}