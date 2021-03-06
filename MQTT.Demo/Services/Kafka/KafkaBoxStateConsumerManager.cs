﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTT.Demo.Core.Kafka;
using MQTT.Demo.Core.Kafka.Consumer;
using MQTT.Demo.Services.Mqtt;

namespace MQTT.Demo.Services.Kafka
{
    public class KafkaBoxStateConsumerManager : KafkaConsumerManagerBase<string, KafkaBoxStateInput>
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Func<string, IMqttService> _mqttServiceFunc;

        public KafkaBoxStateConsumerManager(
            ILogger<KafkaBoxStateConsumerManager> logger,
            Func<string, IMqttService> mqttServiceFunc)
            : base(logger)
        {
            _mqttServiceFunc = mqttServiceFunc;
            _cancellationTokenSource = new CancellationTokenSource();
            _token = _cancellationTokenSource.Token;
        }

        public override void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        protected override async Task Consume(string key, KafkaBoxStateInput value, DateTime utcMessageTimeStamp)
        {
            await _mqttServiceFunc(value.MqttName).BoxStateSubscriptionAsync(value);
        }
    }
}