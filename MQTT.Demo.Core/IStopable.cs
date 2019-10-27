namespace MQTT.Demo.Core
{
    /// <summary>
    ///     此接口在程序结束时调用
    /// </summary>
    public interface IStopable
    {
        void Stop();
    }
}