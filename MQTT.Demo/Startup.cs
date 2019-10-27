using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTT.Demo.Core;
using MQTT.Demo.Core.Kafka.Models;
using MQTT.Demo.Core.Mqtt;
using MQTT.Demo.Services.Https;
using MQTT.Demo.Services.Kafka;
using MQTT.Demo.Services.MessageHandler;

namespace MQTT.Demo
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Startup> _logger;

        public Startup(ILogger<Startup> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        
        public IList<IStopable> Start(IServiceScope serviceScope)
        {
            try
            {
                return StartCore(serviceScope);
            }
            catch (Exception e)
            {
                _logger.LogCritical("Start error", e);
                throw;
            }
        }
        
        private IList<IStopable> StartCore(IServiceScope serviceScope)
        {
            var stopables = new List<IStopable>();
            
            stopables.AddRange( StartMqtt(serviceScope));
            stopables.AddRange(StartConsumers(serviceScope));
            stopables.AddRange(StartBoxStateListen(serviceScope));
            return stopables;
        }
        
        /// <summary>
        ///     运行mqtt客户端
        /// </summary>
        /// <param name="serviceScope"></param>
        /// <returns></returns>
        private IList<IStopable> StartMqtt(IServiceScope serviceScope)
        {
            var mqttSetting = serviceScope.ServiceProvider.GetServices<MqttSetting>().ToArray();
            var mqttManager = serviceScope.ServiceProvider.GetServices<MqttManagerBase>().ToArray();
            for (var i = 0; i < mqttManager.Length; i++) mqttManager[i]?.Init(mqttSetting[i]);
            return mqttManager;
        }
         /// <summary>
        ///     运行kafka消费者
        /// </summary>
        /// <param name="serviceScope"></param>
        /// <returns></returns>
        private IList<IStopable> StartConsumers(IServiceScope serviceScope)
        {
            var consumers = new List<IStopable>();
            var kafkaSetting = serviceScope.ServiceProvider.GetService<KafkaSetting>();

            //    盒子状态消费者
            var kafkaBoxStateConsumerSetting = new KafkaConsumerSetting
            {
                BootstrapServers = kafkaSetting.BootstrapServers,
                GroupId = "BoxState" + kafkaSetting.ConsumerGroupPostFix,
                Topic = kafkaSetting.RequestStateTopic
            };
            for (var i = 0; i < kafkaSetting.BoxStateConsumerCount; i++)
            {
                var boxStateConsumerManager = serviceScope.ServiceProvider.GetService<KafkaBoxStateConsumerManager>();
                consumers.Add(boxStateConsumerManager);
                Task.Factory.StartNew(async () => await boxStateConsumerManager.Init(kafkaBoxStateConsumerSetting),
                    TaskCreationOptions.LongRunning);
            }

            //    监控点kafka消费者
            var kafkaDmonDataConsumerSetting = new KafkaConsumerSetting
            {
                BootstrapServers = kafkaSetting.BootstrapServers,
                GroupId = "DmonData" + kafkaSetting.ConsumerGroupPostFix,
                Topic = kafkaSetting.OpenOrCloseMonitorTopic
            };
            for (var i = 0; i < kafkaSetting.OpenOrCloseMonitorDataConsumerCount; i++)
            {
                var dmonDataConsumerManager = serviceScope.ServiceProvider.GetService<KafkaMonitorDataConsumerManager>();
                consumers.Add(dmonDataConsumerManager);
                Task.Factory.StartNew(
                    async () => await dmonDataConsumerManager.Init(kafkaDmonDataConsumerSetting),
                    TaskCreationOptions.LongRunning);
            }

            //    写值kafka消费者
            var kafkaWriteDataConsumerSetting = new KafkaConsumerSetting
            {
                BootstrapServers = kafkaSetting.BootstrapServers,
                GroupId = "WriteData" + kafkaSetting.ConsumerGroupPostFix,
                Topic = kafkaSetting.WriteDataTopic
            };
            for (var i = 0; i < kafkaSetting.WriteDataConsumerCount; i++)
            {
                var writeDataConsumerManager = serviceScope.ServiceProvider.GetService<KafkaWriteDataConsumerManager>();
                consumers.Add(writeDataConsumerManager);
                Task.Factory.StartNew(
                    async () => await writeDataConsumerManager.Init(kafkaWriteDataConsumerSetting),
                    TaskCreationOptions.LongRunning);
            }

            return consumers;
        }

         /// <summary>
         ///     启动监听Mqtt消息队列的消费者
         /// </summary>
         /// <param name="serviceScope"></param>
         /// <returns></returns>
         private IList<IStopable> StartBoxStateListen(IServiceScope serviceScope)
         {
             //    盒子上线掉线的消息队列消费者
             var boxStateMessageHandler = serviceScope.ServiceProvider.GetService<MqttBoxStateMessageHandler>();
             //    监控点数据消息队列消费者
             var monitorDataMessageHandler =
                 serviceScope.ServiceProvider.GetService<MqttMonitorDataMessageHandler>();

             var mqttSettings = serviceScope.ServiceProvider.GetServices<MqttSetting>();
             foreach (var mqttSetting in mqttSettings)
             {
                 //    设置当前消费者监听消息队列,
                 boxStateMessageHandler.Init(mqttSetting.BoxConnectedTopic, mqttSetting.BoxDisConnectedTopic);
                 monitorDataMessageHandler.Init(mqttSetting.MonitorDataTopic);
             }

             return new List<IStopable> {boxStateMessageHandler, monitorDataMessageHandler};
         }        
    }
}