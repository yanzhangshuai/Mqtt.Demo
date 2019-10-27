using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTT.Demo.Core.MessageQueue;
using MQTTnet;

namespace MQTT.Demo.Core.Mqtt
{
    /// <summary>
    ///     FBox Mqtt服务端  假设有这个服务器协议
    /// </summary>
    [Mqtt(MqttServerType.FBoxMqtt)]
    public class FBoxMqttManager : MqttManagerBase
    {
        private readonly IMessageQueueManager _messageQueueManager;
        private readonly CancellationTokenSource _tokenSource;

        public FBoxMqttManager(ILogger<FBoxMqttManager> logger,
            IMessageQueueManager messageQueueManager) : base(logger)
        {
            _messageQueueManager = messageQueueManager;
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
        }

        public override void Stop()
        {
            _tokenSource.Cancel();
        }

        protected override async Task OnApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs arg)
        {
            //    截取Topic的最后topicName使用作为消息队列的Key
            var topicName = arg.ApplicationMessage.Topic;
            topicName = topicName.Substring(topicName.LastIndexOf('/') + 1);
            //    因为每一次推过来的都是同一个实例,所以在此进行拷贝
            var message =
                JsonSerializer.Deserialize<BoxMqttApplicationMessage>(
                    JsonSerializer.Serialize(arg.ApplicationMessage));

            //    当前Client为盒子号
            message.BoxNo = arg.ClientId;
            message.TopicName = topicName;
            //    将数据添加到消息队列,以便处理
            _messageQueueManager.Enqueue(topicName, message);
            await new ValueTask();
        }
    }
}