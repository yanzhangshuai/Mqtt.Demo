using System;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace MQTT.Demo.Core.Kafka
{
    /// <summary>
    ///     Kafka客户端反序列类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultKafkaConsumerDeserializer<T>:IDeserializer<T>
    {
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            if (data == null) return default;
            var byteArray = data.ToArray();
            return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(byteArray));
        }
    }
}