namespace MQTT.Demo.Core.Kafka.Models
{
    public enum ConsumeOffset : long
    {
        Unspecified = 0,
        Begin = -2,
        End = -1,
        Last = -1001
    }
}