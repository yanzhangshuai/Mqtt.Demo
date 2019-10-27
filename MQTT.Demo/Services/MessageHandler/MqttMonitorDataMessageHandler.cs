using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTT.Demo.Core.Kafka.Models;
using MQTT.Demo.Core.Kafka.Producer;
using MQTT.Demo.Core.MessageQueue;
using MQTT.Demo.Core.Mqtt;
using MQTT.Demo.Services.Kafka;

namespace MQTT.Demo.Services.MessageHandler
{
    public class MqttMonitorDataMessageHandler : MqttMessageHandler
    {
        private readonly IKafkaProducerManager _kafkaProducerManager;
        private readonly KafkaSetting _kafkaSetting;

        public MqttMonitorDataMessageHandler(
            IKafkaProducerManager kafkaProducerManager,
            ILogger<MqttMonitorDataMessageHandler> logger,
            IMessageQueueManager messageQueueManager,
            KafkaSetting kafkaSetting)
            : base(logger, messageQueueManager)
        {
            _kafkaProducerManager = kafkaProducerManager;
            _kafkaSetting = kafkaSetting;
        }

        public override void Stop()
        {
            _kafkaProducerManager?.Stop();
        }

        public override async Task OnMqttDataChanged(EnumQueueMode mode, BoxMqttApplicationMessage applicationMessage)
        {
            try
            {
                //    如果是删除的触发事件,则不需处理
                if (mode == EnumQueueMode.Dequeue) return;

                //    将获取到的数据进行转换为监听数据,然后推送到kafka
                var messageStr = Encoding.UTF8.GetString(applicationMessage.Payload);
                var monitorData = JsonSerializer.Deserialize<FBoxMonitorDataDto>(messageStr);

                var dmonDataConsumerOutputs = monitorData.FBoxMonitorDataInfos.Select(data =>
                    new KafkaMonitorDataOutput
                    {
                        State = data.Value == null || string.IsNullOrWhiteSpace(data.Value.ToString()) ? 1 : 0,
                        DmonName = data.Name,
                        Value = data.Value,
                        TimeStamp = monitorData.Time,
                        BoxBo = applicationMessage.BoxNo,
                        MqttName = MqttServerType.FBoxMqtt
                    }).ToArray();
                await _kafkaProducerManager.PushDataAsync(_kafkaSetting.MonitorDataTopic, applicationMessage.BoxNo,
                    dmonDataConsumerOutputs);
                //    去除当前消费掉的元素
                _messageQueueManager.Dequeue(applicationMessage.TopicName);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
    }
}