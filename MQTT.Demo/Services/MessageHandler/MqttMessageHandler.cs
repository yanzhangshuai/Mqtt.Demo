using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTT.Demo.Core;
using MQTT.Demo.Core.MessageQueue;
using MQTT.Demo.Core.Mqtt;

namespace MQTT.Demo.Services.MessageHandler
{
    public abstract class MqttMessageHandler:IStopable
    {
        protected readonly ILogger _logger;
        protected readonly IMessageQueueManager _messageQueueManager;

        protected MqttMessageHandler(ILogger logger,
            IMessageQueueManager messageQueueManager)
        {
            _logger = logger;
            _messageQueueManager = messageQueueManager;
        }

        public abstract void Stop();

        /// <summary>
        ///     根据topicName进行订阅消息队列事件
        /// </summary>
        /// <param name="topicNames"></param>
        public void Init(params string[] topicNames)
        {
            foreach (var topicName in topicNames) _messageQueueManager.AddHander(topicName, OnMqttDataChanged);
        }

        /// <summary>
        ///     消息处理者
        /// </summary>
        /// <param name="action">
        ///     当前消息来源方式
        /// </param>
        /// <param name="applicationMessage"></param>
        /// <returns></returns>
        public abstract Task OnMqttDataChanged(EnumQueueMode mode, BoxMqttApplicationMessage applicationMessage);
    }
}