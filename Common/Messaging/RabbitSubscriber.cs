using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;
using Prometheus;

namespace Lucent.Common.Messaging
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RabbitSubscriber<T> : IMessageSubscriber<T>
        where T : IMessage
    {
        IConnection _conn;
        IModel _channel;
        AsyncEventingBasicConsumer _consumer;
        string _queueName;
        readonly IMessageFactory _factory;
        readonly IConnectionFactory _connectionFactory;
        readonly ILogger _log;
        readonly string _filter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="connectionFactory"></param>
        /// <param name="log"></param>
        /// <param name="topic"></param>
        /// <param name="filter"></param>
        public RabbitSubscriber(IMessageFactory factory, IConnectionFactory connectionFactory, ILogger log, string topic, string filter)
        {
            _filter = filter;
            _connectionFactory = connectionFactory;
            Topic = topic;
            _log = log;
            _factory = factory;

            SetupChannel();
        }

        void SetupChannel()
        {
            _conn = _connectionFactory.CreateConnection();
            _channel = _conn.CreateModel();

            _channel.ExchangeDeclare(exchange: Topic, type: "topic");
            _queueName = _channel.QueueDeclare(queue: _queueName ?? "", autoDelete: true).QueueName;

            _channel.QueueBind(queue: _queueName,
                  exchange: Topic,
                  routingKey: _filter ?? _factory.WildcardFilter);

            _consumer = new AsyncEventingBasicConsumer(_channel);
            _consumer.Received += ProcessMessage;

            _channel.BasicConsume(queue: _queueName,
                                 autoAck: false,
                                 consumer: _consumer);

        }

        private async Task ProcessMessage(object model, BasicDeliverEventArgs ea)
        {
            using (var ctx = MessageCounters.LatencyHistogram.CreateContext(Topic, "process"))
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
                        if (ea.Body != null)
                            await msg.Load(ea.Body);
                        await OnReceive.Invoke(msg);
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception e)
                    {
                        _log.LogError(e, "Failed to handle message receipt for topic: {0}", Topic);
                        MessageCounters.ErrorCounter.WithLabels(Topic, e.GetType().Name).Inc();
                        _channel.BasicReject(ea.DeliveryTag, ea.Redelivered);
                    }
                }
        }

        /// <inheritdoc/>
        public Func<T, Task> OnReceive { get; set; }

        /// <inheritdoc/>
        public string Topic { get; set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            _channel.Close();
            _conn.Close();
        }
    }
}