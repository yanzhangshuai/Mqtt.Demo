namespace MQTT.Demo.Core.Kafka.Models
{
    public class KafkaSetting
    {
        /// <summary>
        ///     服务器
        /// </summary>
        public string BootstrapServers { get; set; }

        /// <summary>
        ///     Topic前缀
        /// </summary>
        public string TopicPrefix { get; set; }

        /// <summary>
        ///     监控点Topic    生产者
        /// </summary>
        public string MonitorDataTopic { get; set; }

        /// <summary>
        ///     开点关点    消费者
        /// </summary>

        public string OpenOrCloseMonitorTopic { get; set; }

        /// <summary>
        ///     监控点消费者数量
        /// </summary>
        public int OpenOrCloseMonitorDataConsumerCount { get; set; }


        /// <summary>
        ///     获取状态的Topic  生产者
        /// </summary>
        public string RequestStateTopic { get; set; }

        /// <summary>
        ///     盒子状态Topic    消费者
        /// </summary>
        public string BoxStateTopic { get; set; }

        /// <summary>
        ///    盒子状态消费者数量 
        /// </summary>
        public int BoxStateConsumerCount { get; set; }

        /// <summary>
        ///     写值Topic
        /// </summary>
        public string WriteDataTopic { get; set; }

        /// <summary>
        ///     写值消费者数量
        /// </summary>
        public int WriteDataConsumerCount { get; set; }

        /// <summary>
        ///     分组前缀
        /// </summary>
        public string ConsumerGroupPostFix { get; set; }

        /// <summary>
        ///     分区数量
        /// </summary>
        public int TopicPartitionCount { get; set; }
    }
}