using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Lucent.Common.Messaging
{

    internal class RabbitSubscriber<T> : IMessageSubscriber<T>
        where T : IMessage
    {
        readonly IConnection _conn;
        readonly IModel _channel;
        readonly EventingBasicConsumer _consumer;
        readonly string _queueName;
        readonly IMessageFactory _factory;
        readonly ILogger _log;

        public RabbitSubscriber(IMessageFactory factory, IConnection conn, ILogger log, string topic, ushort maxConcurrency, string filter)
        {
            _conn = conn;
            _channel = conn.CreateModel();
            _log = log;
            _factory = factory;
            Topic = topic;

            // Leave defaults if 0
            if (maxConcurrency > 0)
                _channel.BasicQos(maxConcurrency, maxConcurrency, false);

            _channel.ExchangeDeclare(exchange: topic, type: "topic");
            _queueName = _channel.QueueDeclare().QueueName;

            _channel.QueueBind(queue: _queueName,
                  exchange: topic,
                  routingKey: filter ?? "#"); // Match everything

            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += (model, ea) =>
            {
                if (OnReceive != null)
                {
                    try
                    {
                        // Build the message
                        var msg = _factory.CreateMessage<T>();
                        msg.Route = ea.RoutingKey;
                        msg.Timestamp = ea.BasicProperties.Timestamp.UnixTime;
                        msg.CorrelationId = ea.BasicProperties.CorrelationId;
                        msg.Headers = new Dictionary<string, object>();
                        foreach (var header in ea.BasicProperties.Headers ?? new Dictionary<string, object>())
                        {
                            if (header.Value.GetType() == typeof(byte[]))
                                msg.Headers.Add(header.Key, Encoding.UTF8.GetString(header.Value as byte[]));
                            else
                                msg.Headers.Add(header);
                        }
                        msg.FirstDelivery = !ea.Redelivered;
                        msg.ContentType = ea.BasicProperties.ContentType;
                        msg.Load(ea.Body);

                        OnReceive.Invoke(msg);
                    }
                    catch (Exception e)
                    {
                        _log.LogError(e, "Failed to handle message receipt for topic: {0}", Topic);
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