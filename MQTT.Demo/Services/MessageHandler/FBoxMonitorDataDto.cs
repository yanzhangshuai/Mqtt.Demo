using System;
using System.Collections.Generic;

namespace MQTT.Demo.Services.MessageHandler
{
    public class FBoxMonitorDataDto
    {
        public DateTime Time { get; set; }
        public IList<FBoxMonitorDataInfo> FBoxMonitorDataInfos { get; set; }
    }

    public class FBoxMonitorDataInfo
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}