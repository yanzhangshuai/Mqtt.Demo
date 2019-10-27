using MQTT.Demo.Core.Kafka.Models;

namespace MQTT.Demo.Services.Kafka
{
    public class KafkaBoxStateOutput : KafkaProducerSetting
    {
        public string BoxNo { get; set; }

        public int BoxState { get; set; }
    }
}