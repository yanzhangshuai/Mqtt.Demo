using System;

namespace MQTT.Demo.Core.Mqtt
{
    /// <summary>
    ///     此特性标志是哪个mqtt服务器
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class MqttAttribute : Attribute
    {
        public MqttAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}