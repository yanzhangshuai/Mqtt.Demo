namespace MQTT.Demo.Core.Kafka.Models
{
    public class KafkaConsumerSetting
    {
        public string BootstrapServers { get; set; }

        public string GroupId { get; set; }

        public string Topic { get; set; }

        public ConsumeOffset ConsumeOffset { get; set; }
    }
}