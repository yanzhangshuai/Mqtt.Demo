using System.Collections.Generic;
using System.Threading.Tasks;

namespace MQTT.Demo.Core.MessageQueue
{
    /// <summary>
    ///     附带事件触发的消息队列
    ///     在入队和出队时进行触发事件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class MessageQueue<T> : Queue<T>
    {
        internal event QueueMessageDelegate<T> _messageChangedHandler;

        public new void Enqueue(T item)
        {
            base.Enqueue(item);
            _messageChangedHandler?.Invoke(EnumQueueMode.Enqueue, item);
        }

        public new void Dequeue()
        {
            var item = base.Dequeue();
            _messageChangedHandler?.Invoke(EnumQueueMode.Dequeue, item);
        }
    }

    public delegate Task QueueMessageDelegate<in T>(EnumQueueMode action, T item);
}