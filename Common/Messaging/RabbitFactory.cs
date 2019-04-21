using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Threading;
using Lucent.Common.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Lucent.Common.Messaging
{
    /// <summary>
    /// Messaging factory for using RabbitMQ
    /// </summary>
    public class RabbitFactory : IMessageFactory
    {
        private ConnectionFactory _factory;
        private Dictionary<string, RabbitCluster> _clusters;
        private ISerializationContext _serializationContext;
        readonly ILogger<RabbitFactory> _log;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="options">Configuration parameters for the environment</param>
        /// <param name="serializationContext"></param>
        /// <param name="log"></param>
        public RabbitFactory(IOptions<RabbitConfiguration> options, ISerializationContext serializationContext, ILogger<RabbitFactory> log)
        {
            var configuration = options.Value;
            _log = log;

            _factory = new ConnectionFactory
            {
                DispatchConsumersAsync = true,
                HostName = configuration.Host,
                UserName = configuration.User,
                Password = configuration.Credentials,
            };

            _clusters = new Dictionary<string, RabbitCluster>();
            _serializationContext = serializationContext;

            // Build connections to each of the clusters
            foreach (var cluster in configuration.Clusters)
            {
                _clusters.Add(cluster.Key, cluster.Value);
            }
        }

        /// <inheritdoc/>
        public string WildcardFilter => "#";

        /// <inheritdoc/>
        public T CreateMessage<T>()
            where T : IMessage
        {
            var constructor = typeof(T).GetConstructors().FirstOrDefault(ci =>
            {
                return ci.GetParameters().Any(p => p.ParameterType.Equals(typeof(ISerializationContext)));
            });

            return constructor != null ? (T)constructor.Invoke(new object[] { _serializationContext }) : (T)typeof(T).GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
        }

        /// <inheritdoc />
        public IMessagePublisher CreatePublisher(string topic)
        {
            for (var i = 0; i < 5; ++i)
            {
                try
                {
                    return new RabbitPublisher(this, _log, _factory.CreateConnection(), topic);
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Failed to create publisher, retrying...");
                    Thread.Sleep(100);
                }
            }
            throw new LucentException("Failed to create publisher");
        }

        /// <inheritdoc />
        public IMessagePublisher CreatePublisher(string cluster, string topic)
        {
            for (var i = 0; i < 5; ++i)
            {
                try
                {
                    return new RabbitHttpPublisher(this, _log, _clusters[cluster], _serializationContext, topic);
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Failed to create publisher, retrying...");
                    Thread.Sleep(100);
                }
            }
            throw new LucentException("Failed to create publisher");
        }

        /// <inheritdoc />
        public IEnumerable<string> GetClusters() => _clusters.Keys.ToArray();

        /// <inheritdoc />
        public IMessageSubscriber<T> CreateSubscriber<T>(string topic)
            where T : IMessage
        {
            for (var i = 0; i < 5; ++i)
            {
                try
                {
                    _log.LogInformation("Creating subscriber for {0} ({1})", topic, "null");
                    return new RabbitSubscriber<T>(this, _factory.CreateConnection(), _log, topic, null);
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Failed to create subscriber, retrying...");
                    Thread.Sleep(100);
                }
            }
            throw new LucentException("Failed to create subscriber");
        }

        /// <inheritdoc />
        public IMessageSubscriber<T> CreateSubscriber<T>(string topic, string filter)
            where T : IMessage
        {
            for (var i = 0; i < 5; ++i)
            {
                try
                {
                    _log.LogInformation("Creating subscriber for {0} ({1})", topic, filter ?? "null");
                    return new RabbitSubscriber<T>(this, _factory.CreateConnection(), _log, topic, filter);
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Failed to create subscriber, retrying...");
                    Thread.Sleep(100);
                }
            }

            throw new LucentException("Failed to create subscriber");
        }
    }
}