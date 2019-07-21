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
        IConnection _conn;
        IModel _channel;
        readonly IMessageFactory _factory;
        readonly IConnectionFactory _connectionFactory;
        readonly ILogger _log;
        readonly bool _broadcastEnabled;

        /// <inheritdoc/>
        public string Topic { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="log"></param>
        /// <param name="connectionFactory"></param>
        /// <param name="topic"></param>
        /// <param name="broadcastEnabled"></param>
        public RabbitPublisher(IMessageFactory factory, ILogger log, IConnectionFactory connectionFactory, string topic, bool broadcastEnabled = false)
        {
            _connectionFactory = connectionFactory;
            _factory = factory;
            _log = log;
            _broadcastEnabled = broadcastEnabled;
            Topic = topic;
            Setup();
        }

        bool Setup()
        {
            try
            {
                _conn = _connectionFactory.CreateConnection();
                _channel = _conn.CreateModel();
                _channel.ExchangeDeclare(Topic, "topic");
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to create connection");
                _conn = null;
                _channel = null;
            }
            return false;
        }

        void Teardown()
        {
            try
            {
                _channel.Close();
                _channel.Dispose();
                _conn.Close();
                _conn.Dispose();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed teardown");
            }
            _conn = null;
            _channel = null;
        }

        /// <inheritdoc/>
        public async Task<bool> TryPublish(IMessage message)
        {
            if (_conn == null && !Setup())
                return false;

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
                    Teardown();
                }

            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> TryBroadcast(IMessage message)
        {
            if(_broadcastEnabled)
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
        public void Dispose() => Teardown();
    }
}