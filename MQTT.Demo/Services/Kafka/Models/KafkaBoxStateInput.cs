namespace MQTT.Demo.Services.Kafka
{
    public class KafkaBoxStateInput
    {
        public string MqttName { get; set; }

        public string BoxNo { get; set; }

        public int Action { get; set; }
    }
}