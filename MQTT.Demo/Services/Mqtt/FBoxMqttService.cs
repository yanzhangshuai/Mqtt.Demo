using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTT.Demo.Core.Kafka.Models;
using MQTT.Demo.Core.Kafka.Producer;
using MQTT.Demo.Core.Mqtt;
using MQTT.Demo.Services.Https;
using MQTT.Demo.Services.Kafka;

namespace MQTT.Demo.Services.Mqtt
{
    [Mqtt(MqttServerType.FBoxMqtt)]
    public class FBoxMqttService : IMqttService
    {
        /// <summary>
        ///     开关点和获取盒子状态只需要发布一次,所以进行缓存,避免重复
        /// </summary>
        private static readonly ConcurrentDictionary<string, int> _boxStateBoxNoCache;

        private static readonly ConcurrentBag<string> _boxOpenOrCloseMonitorDataBoxNoCache;
        private readonly BoxStateHttpClient _boxStateHttpClient;
        private readonly IKafkaProducerManager _kafkaProducerManager;
        private readonly KafkaSetting _kafkaSetting;
        private readonly ILogger<FBoxMqttService> _logger;

        private readonly Func<string, MqttManagerBase> _mqttManagerFunc;
        private readonly MqttSetting _mqttSetting;

        #region 初始化缓存集合

        static FBoxMqttService()
        {
            try
            {
                var boxStateBoxNoCacheStr =
                    File.ReadAllText($"./Cache/{nameof(FBoxMqttService)}.{nameof(_boxStateBoxNoCache)}.txt");
                _boxStateBoxNoCache =
                    JsonSerializer.Deserialize<ConcurrentDictionary<string, int>>(boxStateBoxNoCacheStr);
            }
            catch
            {
                _boxStateBoxNoCache = new ConcurrentDictionary<string, int>();
            }

            try
            {
                var boxOpenOrCloseDmonDataBoxNoCacheStr =
                    File.ReadAllText(
                        $"./Cache/{nameof(FBoxMqttService)}.{nameof(_boxOpenOrCloseMonitorDataBoxNoCache)}.txt");
                _boxOpenOrCloseMonitorDataBoxNoCache =
                    JsonSerializer.Deserialize<ConcurrentBag<string>>(boxOpenOrCloseDmonDataBoxNoCacheStr);
            }
            catch
            {
                _boxOpenOrCloseMonitorDataBoxNoCache = new ConcurrentBag<string>();
            }
        }

        #endregion

        public FBoxMqttService(
            Func<string, MqttManagerBase> mqttManagerFunc,
            MqttSetting mqttSetting,
            BoxStateHttpClient boxStateHttpClient,
            ILogger<FBoxMqttService> logger,
            IKafkaProducerManager kafkaProducerManager,
            KafkaSetting kafkaSetting
        )
        {
            _mqttManagerFunc = mqttManagerFunc ?? throw new ArgumentNullException(nameof(_mqttManagerFunc));
            _mqttSetting = mqttSetting ?? throw new ArgumentNullException(nameof(_mqttSetting));
            _boxStateHttpClient = boxStateHttpClient ?? throw new ArgumentNullException(nameof(_boxStateHttpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(_logger));
            _kafkaProducerManager =
                kafkaProducerManager ?? throw new ArgumentNullException(nameof(_kafkaProducerManager));
            _kafkaSetting = kafkaSetting ?? throw new ArgumentNullException(nameof(kafkaSetting));
        }

        public async Task BoxStateSubscriptionAsync(KafkaBoxStateInput input)
        {
            Console.WriteLine("这是FBoxMqttService的BoxStateSubscriptionAsync方法:" + JsonSerializer.Serialize(input));
            //    如果当前盒子已经推送过一次,那么就不在需要进行访问API
            if (_boxStateBoxNoCache.ContainsKey(input.BoxNo)) return;

            //    盒子状态第一次调用此方法时需要立即进行返回此盒子状态,所以需要请求api进行获取当前盒子状态,然后返回
            try
            {
                //    获取盒子状态,然后推送kafka
                var isConnected = await _boxStateHttpClient.IsConnectedAsync(input.BoxNo);
                await _kafkaProducerManager.PushDataAsync(_kafkaSetting.BoxStateTopic,
                    input.BoxNo,
                    new KafkaBoxStateOutput
                    {
                        MqttName = input.MqttName,
                        //    在线为1,离线为3
                        BoxState = isConnected ? 1 : 3,
                        BoxNo = input.BoxNo
                    });
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
                _logger.LogError(e.Message);
            }

            //    然后再进行向mqtt进行发送订阅盒子状态消息
            //  获取当前mqtt实例
            var mqttManager = _mqttManagerFunc(input.MqttName);

            var connectedTopic = _mqttSetting.BoxConnectedTopicPrefix + input.BoxNo + "/" +
                                 _mqttSetting.BoxConnectedTopic;
            var disConnectedTopic = _mqttSetting.BoxConnectedTopicPrefix + input.BoxNo + "/" +
                                    _mqttSetting.BoxDisConnectedTopic;
            await mqttManager.SubscribeAsync(connectedTopic, disConnectedTopic);

            //    缓存当前数据
            _boxStateBoxNoCache.TryAdd(input.BoxNo, input.Action);
            await File.WriteAllTextAsync($"./Cache/{nameof(FBoxMqttService)}.{nameof(_boxStateBoxNoCache)}.txt",
                JsonSerializer.Serialize(_boxStateBoxNoCache));
        }

        public async Task OpenOrCloseMonitorDataSubscriptionAsync(KafkaOpenOrCloseMonitorDataInput input)
        {
            Console.WriteLine("这是FBoxMqttService的BoxOpenOrCloseDmonDataSubscriptionAsync方法:" +
                              JsonSerializer.Serialize(input));
            var mqttManager = _mqttManagerFunc(input.MqttName);

            //    获取当前开关点盒子topic
            var topicNames = input.MonitorData.Select(y => y.BoxNo).Distinct().Select(boxNo =>
            {
                var topicPrefix = _mqttSetting.MqttTopicPrefix.Replace("${SN}", boxNo);
                return topicPrefix + _mqttSetting.PauseTopic;
            });
            IList<Task> mqttOperaTasks = new List<Task>();

            if (input.Action == 0)
            {
                //    关点
                foreach (var topicName in topicNames)
                    mqttOperaTasks.Add(mqttManager.PublishAsync(topicName, "Enable"));
                await Task.WhenAll(mqttOperaTasks);
                return;
            }

            //    开点,判断其当前盒子是否已被订阅监听数据,如果没有订阅,那么就进行订阅
            //    key为盒子号,value为订阅
            var subMonitorTopicNames = new Dictionary<string, string>();
            input.MonitorData.Select(y => y.BoxNo).Distinct().ToList().ForEach(boxNo =>
            {
                if (_boxOpenOrCloseMonitorDataBoxNoCache.Contains(boxNo)) return;
                var topicPrefix = _mqttSetting.MqttTopicPrefix.Replace("${SN}", boxNo);
                var topicName = topicPrefix + _mqttSetting.MonitorDataTopic;
                subMonitorTopicNames.Add(boxNo, topicName);
            });
            if (subMonitorTopicNames.Any())
            {
                //    如果没有订阅,那么就进行订阅,并添加此盒子到缓存集合中
                mqttOperaTasks.Add(mqttManager.SubscribeAsync(subMonitorTopicNames.Values.ToArray()));
                foreach (var (key, _) in subMonitorTopicNames)
                    _boxOpenOrCloseMonitorDataBoxNoCache.Add(key);
                mqttOperaTasks.Add(File.WriteAllTextAsync(
                    $"./Cache/{nameof(FBoxMqttService)}.{nameof(_boxOpenOrCloseMonitorDataBoxNoCache)}.txt",
                    JsonSerializer.Serialize(_boxOpenOrCloseMonitorDataBoxNoCache)));
            }

            foreach (var topicName in topicNames) mqttOperaTasks.Add(mqttManager.PublishAsync(topicName, "Disable"));
            await Task.WhenAll(mqttOperaTasks);
        }


        public async Task WriteDataAsync(KafkaWriteDataInput input)
        {
            Console.WriteLine("这是FBoxMqttService的WriteDataAsync方法:" + JsonSerializer.Serialize(input));
            //  获取当前mqtt实例
            var mqttManager = _mqttManagerFunc(input.MqttName);
            var writeData = input.MonitorData.Select(data =>
                new
                {
                    Version = 10,
                    Data = new List<object>
                    {
                        new
                        {
                            name = data.DataInfo["Name"],
                            value = data.Value
                        }
                    }
                }
            );
            var topicPrefix = _mqttSetting.MqttTopicPrefix.Replace("${SN}", input.BoxNo);
            var topicName = topicPrefix + _mqttSetting.WriteDataTopic;
            IList<Task> writeDataTasks = writeData
                .Select(data => mqttManager.PublishAsync(topicName, JsonSerializer.Serialize(data))).ToList();
            await Task.WhenAll(writeDataTasks);
        }
    }
}