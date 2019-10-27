namespace MQTT.Demo.Core.Mqtt
{
    public class MqttSetting
    {
        /// <summary>
        ///     mqtt服务器IP
        /// </summary>
        public string ServerAddress { get; set; }

        /// <summary>
        ///     mqtt服务器端口号
        /// </summary>
        public int ServerPort { get; set; }

        /// <summary>
        ///     是否清除Session
        /// </summary>
        public bool CleanSession { get; set; }

        public string ServerUserName { get; set; }

        public string ServerPassword { get; set; }

        /// <summary>
        ///     连接前缀Topic,因为连接和断开连接可能与其它不一致
        ///     不过这个也是根据mqtt服务器不同而不同
        ///     在此未做区分别的mqtt
        /// </summary>
        public string BoxConnectedTopicPrefix { get; set; }

        /// <summary>
        ///     连接Topic
        /// </summary>
        public string BoxConnectedTopic { get; set; }

        /// <summary>
        ///     断开连接Topic
        /// </summary>
        public string BoxDisConnectedTopic { get; set; }

        /// <summary>
        ///     mqtt topic前缀
        /// </summary>
        public string MqttTopicPrefix { get; set; }

        /// <summary>
        ///     是否暂停推送数据Topic
        /// </summary>
        public string PauseTopic { get; set; }

        /// <summary>
        ///     监控点topic
        /// </summary>
        public string MonitorDataTopic { get; set; }

        /// <summary>
        ///     写值topic
        /// </summary>
        public string WriteDataTopic { get; set; }

        /// <summary>
        ///     最大重试次数
        /// </summary>

        public int MaxRetryConnectedCount { get; set; }
    }
}