using System.Threading.Tasks;
using MQTT.Demo.Services.Kafka;

namespace MQTT.Demo.Services.Mqtt
{
    /// <summary>
    ///     mqtt业务service,
    ///     每一个action为一个业务操作
    ///     将每个mqtt服务器的操作集中在一起,方便管理
    /// </summary>
    public interface IMqttService
    {
        /// <summary>
        ///     获取其盒子状态
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task BoxStateSubscriptionAsync(KafkaBoxStateInput input);

        /// <summary>
        ///     开点或关点
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task OpenOrCloseMonitorDataSubscriptionAsync(KafkaOpenOrCloseMonitorDataInput input);

        /// <summary>
        ///     写值
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task WriteDataAsync(KafkaWriteDataInput input);
    }
}