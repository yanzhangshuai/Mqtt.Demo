using System;
using MQTT.Demo.Core.Kafka.Models;

namespace MQTT.Demo.Services.Kafka
{
    public class KafkaMonitorDataOutput : KafkaProducerSetting
    {
        public string DmonName { get; set; }

        public string BoxBo { get; set; }

        public object Value { get; set; }

        public int State { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}