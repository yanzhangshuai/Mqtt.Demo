using System.Collections.Generic;
using MQTT.Demo.Core.Mqtt;

namespace MQTT.Demo.Core.MessageQueue
{
    public class DefaultMessageQueueManager : IMessageQueueManager
    {
        private static readonly Dictionary<string, MessageQueue<BoxMqttApplicationMessage>> _messageQueueDictionary =
            new Dictionary<string, MessageQueue<BoxMqttApplicationMessage>>();

        public void Enqueue(string key, BoxMqttApplicationMessage message)
        {
            AddQueueMessageOfIsNull(key);
            _messageQueueDictionary[key].Enqueue(message);
        }

        public void Dequeue(string key)
        {
            if (!_messageQueueDictionary.ContainsKey(key)) return;
            _messageQueueDictionary[key].Dequeue();
        }

        public void AddHander(string key, QueueMessageDelegate<BoxMqttApplicationMessage> handler)
        {
            AddQueueMessageOfIsNull(key);
            _messageQueueDictionary[key]._messageChangedHandler += handler;
        }

        public void RemoveHander(string key, QueueMessageDelegate<BoxMqttApplicationMessage> handler)
        {
            if (!_messageQueueDictionary.ContainsKey(key)) return;
            _messageQueueDictionary[key]._messageChangedHandler -= handler;
        }

        private void AddQueueMessageOfIsNull(string key)
        {
            //    暂未实现多线程同步
            if (!_messageQueueDictionary.ContainsKey(key))
                _messageQueueDictionary[key] = new MessageQueue<BoxMqttApplicationMessage>();
        }
    }
}