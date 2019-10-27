using System.Threading.Tasks;
using MQTT.Demo.Core.Kafka.Models;

namespace MQTT.Demo.Core.Kafka.Producer
{
    public interface IKafkaProducerManager : IStopable
    {
        Task PushDataAsync<T>(string topicName, string key, params T[] value)
            where T : KafkaProducerSetting;
    }
}