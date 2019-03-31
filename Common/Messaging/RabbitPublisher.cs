using System.Collections.Generic;
using RabbitMQ.Client;
using System.Threading.Tasks;
using Prometheus;
using Microsoft.Extensions.Logging;
using System;

namespace Lucent.Common.Messaging
{
    /// <summary>
    /// 
    /// </summary>
    public class RabbitPublisher : IMessagePublisher
    {
        readonly IConnection _conn;
        readonly IModel _channel;
        readonly IMessageFactory _factory;
        readonly ILogger _log;

        /// <inheritdoc/>
        public string Topic { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="log"></param>
        /// <param name="conn"></param>
        /// <param name="topic"></param>
        public RabbitPublisher(IMessageFactory factory, ILogger log, IConnection conn, string topic)
        {
            _conn = conn;
            _factory = factory;
            _log = log;
            _channel = conn.CreateModel();
            _channel.ExchangeDeclare(topic, "topic");
            Topic = topic;
        }

        /// <inheritdoc/>
        public async Task<bool> TryPublish(IMessage message)
        {
            using (var ctx = MessageCounters.LatencyHistogram.CreateContext(Topic, "publish"))
                try
                {
                    var props = _channel.CreateBasicProperties();

                    if (!string.IsNullOrEmpty(message.CorrelationId))
                        props.CorrelationId = message.CorrelationId;

                    props.ContentType = message.ContentType;

                    foreach (var header in message.Headers ?? new Dictionary<string, object>())
                        props.Headers.Add(header);

                    _channel.BasicPublish(exchange: Topic,
                             routingKey: message.Route ?? "undefined",
                             basicProperties: props,
                             body: await message.ToBytes());

                    return true;
                }
                catch (Exception e)
                {
                    _log.LogWarning(e, "Failed to publish message on {0}", Topic);
                    MessageCounters.ErrorCounter.WithLabels(Topic, e.GetType().Name).Inc();
                }

            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> TryBroadcast(IMessage message)
        {
            using (var ctx = MessageCounters.LatencyHistogram.CreateContext(Topic, "broadcast"))
                try
                {
                    var res = true;
                    foreach (var cluster in _factory.GetClusters())
                        res &= await _factory.CreatePublisher(cluster, Topic).TryPublish(message);
                    return res;
                }
                catch (Exception e)
                {
                    _log.LogWarning(e, "Failed to publish message on {0}", Topic);
                    MessageCounters.ErrorCounter.WithLabels(Topic, e.GetType().Name).Inc();
                }

            return false;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _channel.Close();
            _conn.Close();
        }
    }
}