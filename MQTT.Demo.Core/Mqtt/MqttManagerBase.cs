using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;

namespace MQTT.Demo.Core.Mqtt
{
    public abstract class MqttManagerBase : IStopable
    {
        protected readonly ILogger _logger;
        protected IMqttClient _mqttClient;
        protected IMqttClientOptions _mqttClientOptions;
        protected CancellationToken _token;

        private int _currentRetryConnectedNum = 1;
        private int _maxRetryConnectedCount = 10;

        protected MqttManagerBase(ILogger logger)
        {
            _logger = logger;
        }

        public abstract void Stop();

        public async Task Init(MqttSetting setting)
        {
            //    设置最大连接数量,默认为10
            _maxRetryConnectedCount = setting.MaxRetryConnectedCount > 0
                ? setting.MaxRetryConnectedCount
                : _maxRetryConnectedCount;
            _mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(setting.ServerAddress, setting.ServerPort)
                .WithCredentials(setting.ServerUserName, setting.ServerPassword)
                .WithClientId(Guid.NewGuid().ToString())
                .WithCleanSession(setting.CleanSession)
                .Build();
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            _mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnDisconnected);
            _mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnConnected);
            //    mqtt数据接收方法,mqtt推送的数据使用此方法进行接收
            _mqttClient.ApplicationMessageReceivedHandler =
                new MqttApplicationMessageReceivedHandlerDelegate(OnApplicationMessageReceived);
            await _mqttClient.ConnectAsync(_mqttClientOptions, _token);
        }

        protected abstract Task OnApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs arg);

        public async Task SubscribeAsync(params string[] topicNames)
        {
            try
            {
                if (null == topicNames || !topicNames.Any()) return;
                var topicFilters = topicNames.Select(topicName => new TopicFilterBuilder()
                    .WithTopic(topicName).Build()).ToArray();
                await _mqttClient.SubscribeAsync(topicFilters);
            }
            catch (Exception ex)
            {
                Array.ForEach(topicNames, topicName =>
                {
                    _logger.LogError($"${topicName}主题订阅异常:{ex.Message}");
                    Console.WriteLine($"${topicName}主题订阅异常:{ex.Message}");
                });
            }
        }

        public async Task UnSubscribeAsync(params string[] topicNames)
        {
            try
            {
                await _mqttClient.UnsubscribeAsync(topicNames);
            }
            catch (Exception ex)
            {
                Array.ForEach(topicNames, topicName =>
                {
                    _logger.LogError($"${topicName}主题取消订阅异常:{ex.Message}");
                    Console.WriteLine($"${topicName}主题取消订阅异常:{ex.Message}");
                });
            }
        }

        public async Task PublishAsync(string topicName, string value)
        {
            try
            {
                await _mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                    .WithTopic(topicName)
                    .WithPayload(value)
                    .WithExactlyOnceQoS()
                    .Build());
            }
            catch (Exception ex)
            {
                _logger.LogError($"向${topicName}发布消息失败:{ex.Message}");
                Console.WriteLine($"向${topicName}发布消息失败:{ex.Message}");
            }
        }

        private void OnConnected(MqttClientConnectedEventArgs args)
        {
            _logger.LogInformation($"MQTT客户端: {_mqttClientOptions.ClientId} 连接成功");
            Console.WriteLine($"MQTT客户端: {_mqttClientOptions.ClientId} 连接成功");
            //    重置重试链接次数
            _currentRetryConnectedNum = 1;
        }

        private async Task OnDisconnected(MqttClientDisconnectedEventArgs arg)
        {
            _logger.LogError($"MQTT服务器连接断开,正在尝试第{_currentRetryConnectedNum}次重新连接");
            Console.WriteLine($"MQTT服务器连接断开,正在尝试第{_currentRetryConnectedNum}次重新连接");
            if (_currentRetryConnectedNum >= _maxRetryConnectedCount)
            {
                _logger.LogError($"MQTT服务器重新连接失败");
                Console.WriteLine($"MQTT服务器重新连接失败");
                return;
            }

            _currentRetryConnectedNum++;
            await _mqttClient.ConnectAsync(_mqttClientOptions, _token);
        }
    }
}