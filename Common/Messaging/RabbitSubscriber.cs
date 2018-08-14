using System;
using System.Collections.Generic;
using System.IO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Lucent.Common.Messaging
{

    internal class RabbitSubscriber<T> : IMessageSubscriber<T>
        where T : IMessage, new()
    {
        readonly IConnection _conn;
        readonly IModel _channel;
        readonly EventingBasicConsumer _consumer;
        readonly string _queueName;

        public RabbitSubscriber(IConnection conn, string topic, ushort maxConcurrency)
        {
            _conn = conn;
            _channel = conn.CreateModel();
            Topic = topic;

            // Leave defaults if 0
            if (maxConcurrency > 0)
                _channel.BasicQos(maxConcurrency, maxConcurrency, false);

            _channel.ExchangeDeclare(exchange: topic, type: "fanout");
            _queueName = _channel.QueueDeclare().QueueName;

            _channel.QueueBind(queue: _queueName,
                  exchange: topic,
                  routingKey: "");

            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += (model, ea) =>
            {
                if (OnReceive != null)
                {
                    try
                    {
                        var msg = new T();
                        msg.Load(ea.Body);
                        OnReceive.Invoke(msg);
                    }
                    catch
                    {

                    }
                }
            };

            _channel.BasicConsume(queue: _queueName,
                                 autoAck: true,
                                 consumer: _consumer);
        }

        public Action<T> OnReceive { get; set; }

        public string Topic { get; set; }
        public void Dispose()
        {
            _channel.Close();
            _conn.Close();
        }
    }
}