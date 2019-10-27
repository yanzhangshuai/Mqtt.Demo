using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using MQTT.Demo.Core.Kafka.Models;

namespace MQTT.Demo.Core.Kafka.Consumer
{
    public abstract class KafkaConsumerManagerBase<T1, T2> : IStopable
    {
        private IConsumer<T1, T2> _consumer;
        protected IDeserializer<T1> _keyDeserializer;
        protected CancellationToken _token;
        protected IDeserializer<T2> _valueDeserializer;

        protected KafkaConsumerManagerBase(ILogger logger)
        {
            Logger = logger;
            //    默认设置Kafka反序列化类型
            _valueDeserializer = new DefaultKafkaConsumerDeserializer<T2>();
        }

        protected ILogger Logger { get; set; }
        public abstract void Stop();

        /// <summary>
        ///     初始化当前客户端
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public async Task Init(KafkaConsumerSetting setting)
        {
            var config = new ConsumerConfig
            {
                //    设置当前Kafka客户端分组,
                //    只要不更改group.id，每次重新消费kafka，都是从上次消费结束的地方继续开始
                GroupId = setting.GroupId,
                BootstrapServers = setting.BootstrapServers,
                EnableAutoCommit = true,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            var consuming = true;
            var builder = new ConsumerBuilder<T1, T2>(config).SetErrorHandler((_, e) =>
            {
                Logger.LogError($"Error: {e.Reason}");
                consuming = !e.IsFatal;
            });
            if (setting.ConsumeOffset != ConsumeOffset.Unspecified)
                builder.SetPartitionsAssignedHandler((c, partitions) =>
                {
                    Logger.LogInformation($"Assigned partitions: [{string.Join(", ", partitions)}]");
                    return partitions.Select(y =>
                        new TopicPartitionOffset(y, new Offset((long) setting.ConsumeOffset)));
                });

            if (_keyDeserializer != null)
                builder.SetKeyDeserializer(_keyDeserializer);
            if (_valueDeserializer != null)
                builder.SetValueDeserializer(_valueDeserializer);

            using (_consumer = builder.Build())
            {
                _consumer.Subscribe(setting.Topic);
                while (consuming && !_token.IsCancellationRequested)
                    try
                    {
                        var cr = _consumer.Consume(_token);
                        try
                        {
                            await Consume(cr.Key, cr.Value, cr.Timestamp.UtcDateTime);
                        }
                        catch (Exception e)
                        {
                            Logger.LogError("KafkaConsumerManager.Consume error", e);
                        }
                    }
                    catch (ConsumeException e)
                    {
                        Logger.LogError($"Error occured: {e.Error.Reason}");
                    }
            }
        }

        protected abstract Task Consume(T1 key, T2 value, DateTime utcMessageTimeStamp);
    }
}