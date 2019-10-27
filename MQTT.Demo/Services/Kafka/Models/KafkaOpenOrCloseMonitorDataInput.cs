using System.Collections.Generic;
using MQTT.Demo.Core.Kafka.Models;

namespace MQTT.Demo.Services.Kafka
{
    public class KafkaOpenOrCloseMonitorDataInput
    {
        /// <summary>
        ///    对应的mqtt服务器名称
        /// </summary>
        public string MqttName { get; set; }

        /// <summary>
        ///     操作.1.开点,0 关点
        /// </summary>

        public int Action { get; set; }

        /// <summary>
        ///     监控点信息
        /// </summary>
        public IList<MonitorDataInfo> MonitorData { get; set; }

        public class MonitorDataInfo
        {
            /// <summary>
            ///     监控点名称
            /// </summary>
            public string MonitorDataName { get; set; }

            /// <summary>
            ///     盒子号
            /// </summary>
            public string BoxNo { get; set; }
        }
    }
}