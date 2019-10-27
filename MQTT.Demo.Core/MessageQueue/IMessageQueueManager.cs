using MQTT.Demo.Core.Mqtt;

namespace MQTT.Demo.Core.MessageQueue
{
    public interface IMessageQueueManager
    {
        void Enqueue(string key, BoxMqttApplicationMessage message);

        /// <summary>
        ///     出列
        /// </summary>
        /// <param name="key"></param>
        void Dequeue(string key);

        /// <summary>
        ///     添加消息队列触发事件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="handler"></param>
        void AddHander(string key, QueueMessageDelegate<BoxMqttApplicationMessage> handler);

        /// <summary>
        ///     去除消息队列触发事件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="handler"></param>
        void RemoveHander(string key, QueueMessageDelegate<BoxMqttApplicationMessage> handler);
    }
}