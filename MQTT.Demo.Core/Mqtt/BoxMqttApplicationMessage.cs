using MQTTnet;

namespace MQTT.Demo.Core.Mqtt
{
    public class BoxMqttApplicationMessage : MqttApplicationMessage
    {
        /// <summary>
        ///     当前盒子序列号
        /// </summary>
        public string BoxNo { get; set; }

        /// <summary>
        ///     Topic名称
        /// </summary>
        public string TopicName { get; set; }
    }
}