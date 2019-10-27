using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MQTT.Demo.Core.Kafka.Producer;
using MQTT.Demo.Core.MessageQueue;
using MQTT.Demo.Services.Https;
using MQTT.Demo.Services.Kafka;
using MQTT.Demo.Services.MessageHandler;
using MQTT.Demo.Services.Models;
using MQTT.Demo.Core.Kafka.Models;
using MQTT.Demo.Core.Mqtt;
using MQTT.Demo.Services.Mqtt;

namespace MQTT.Demo
{
    public class DiBuilder
    {
        public static void Build(IServiceCollection services, IConfiguration configuration)
        {
            var boxStateHttpSetting = configuration.GetSection("BoxStateHttp").Get<BoxStateHttpSetting>();
            
            BuildKafkaSetting(services, configuration);
            BuildMqtt(services, configuration);

            services
                .AddSingleton<IKafkaProducerManager, DefaultKafkaProducerManager>()
                .AddTransient<KafkaBoxStateConsumerManager>()
                .AddTransient<KafkaMonitorDataConsumerManager>()
                .AddTransient<KafkaWriteDataConsumerManager>()
                .AddTransient<MqttBoxStateMessageHandler>()
                .AddTransient<MqttMonitorDataMessageHandler>()
                .AddSingleton<IMessageQueueManager,DefaultMessageQueueManager>()
                .AddTransient<Startup>()
                .AddHttpClient()
                .AddSingleton(boxStateHttpSetting)
                .AddHttpClient<BoxStateHttpClient>()
                ;
        }
        
        private static void BuildKafkaSetting(IServiceCollection services, IConfiguration configuration)
        {
            //    kafka设置,将kafka所有topic进行与前缀拼接
            var kafkaSetting = configuration.GetSection("Kafka").Get<KafkaSetting>();
            kafkaSetting.MonitorDataTopic = $"{kafkaSetting.TopicPrefix}.{kafkaSetting.MonitorDataTopic}";
            kafkaSetting.OpenOrCloseMonitorTopic = $"{kafkaSetting.TopicPrefix}.{kafkaSetting.OpenOrCloseMonitorTopic}";
            kafkaSetting.BoxStateTopic =
                $"{kafkaSetting.TopicPrefix}.{kafkaSetting.BoxStateTopic}";
            kafkaSetting.RequestStateTopic = $"{kafkaSetting.TopicPrefix}.{kafkaSetting.RequestStateTopic}";

            services.AddSingleton(kafkaSetting);
        }

        private static void BuildMqtt(IServiceCollection services, IConfiguration configuration)
        {
            //    mqttSetting 只做了一个实例,但是应该不止只有一个
            var fBoxMqttSetting = configuration.GetSection("Mqtt:FBox").Get<MqttSetting>();
            services
                .AddSingleton(fBoxMqttSetting)
                //    注入mqtt客户端
                .AddSingleton<MqttManagerBase, FBoxMqttManager>()
                
                //注入Func,使用Func来进行获取指定的mqtt客户端实例
                .AddSingleton(provider =>
                {
                    Func<string, MqttManagerBase> func = n =>
                    {
                        return provider.GetServices<MqttManagerBase>().FirstOrDefault(y =>
                            y.GetType().GetCustomAttribute<MqttAttribute>()?.Name == n);
                    };
                    return func;
                })
                ;

            //   注入mqtt服务器的业务service,同mqtt客户端相同
            services.AddTransient<IMqttService, FBoxMqttService>()
                .AddSingleton(provider =>
                {
                    Func<string, IMqttService> func = n =>
                    {
                        return provider.GetServices<IMqttService>().FirstOrDefault(y =>
                            y.GetType().GetCustomAttribute<MqttAttribute>()?.Name == n);
                    };
                    return func;
                });
        }
    }
}