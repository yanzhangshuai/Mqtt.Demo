namespace MQTT.Demo.Core.Kafka.Models
{
    /// <summary>
    ///     Kafka推送实体的基类,只具有一个MqttName,
    ///     标志当前为哪个Mqtt服务器的数据
    /// </summary>
    public class KafkaProducerSetting
    {
        public string MqttName { get; set; }
    }
}