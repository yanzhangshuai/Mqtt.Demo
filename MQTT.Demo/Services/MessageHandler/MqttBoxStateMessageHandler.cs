using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTT.Demo.Core.Kafka.Models;
using MQTT.Demo.Core.Kafka.Producer;
using MQTT.Demo.Core.MessageQueue;
using MQTT.Demo.Core.Mqtt;

namespace MQTT.Demo.Services.MessageHandler
{
    /// <summary>
    ///     盒子状态消息处理者
    /// </summary>
    public class MqttBoxStateMessageHandler : MqttMessageHandler
    {
        private readonly IKafkaProducerManager _kafkaProducerManager;

        public MqttBoxStateMessageHandler(
            IMessageQueueManager messageQueueManager,
            ILogger<MqttBoxStateMessageHandler> logger,
            IKafkaProducerManager kafkaProducerManager)
            : base(logger, messageQueueManager)
        {
            _kafkaProducerManager = kafkaProducerManager;
        }


        public override void Stop()
        {
            _kafkaProducerManager?.Stop();
        }

        public override Task OnMqttDataChanged(EnumQueueMode mode, BoxMqttApplicationMessage applicationMessage)
        {
            throw new System.NotImplementedException();
        }
    }
}