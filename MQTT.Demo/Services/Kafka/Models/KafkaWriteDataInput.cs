using System.Collections.Generic;

namespace MQTT.Demo.Services.Kafka
{
    public class KafkaWriteDataInput
    {
        public string MqttName { get; set; }

        public string BoxNo { get; set; }

        public IList<WriteDataInfo> MonitorData { get; set; }
    }

    public class WriteDataInfo
    {
        public IDictionary<string, string> DataInfo { get; set; }
        public string Value { get; set; }
    }
}