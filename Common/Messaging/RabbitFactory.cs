using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
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
        private IServiceProvider _provider;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="options">Configuration parameters for the environment</param>
        public RabbitFactory(IOptions<RabbitConfiguration> options, IServiceProvider provider)
        {
            var configuration = options.Value;

            _factory = new ConnectionFactory();
            _factory.HostName = configuration.Host;
            _factory.UserName = configuration.User;
            _factory.Password = configuration.Credentials;

            _clusters = new Dictionary<string, RabbitCluster>();
            _provider = provider;

            // Build connections to each of the clusters
            foreach (var cluster in configuration.Clusters)
            {
                _clusters.Add(cluster.Key, cluster.Value);
            }
        }

        public IMessage CreateMessage()
            => new LucentMessage();

        public T CreateMessage<T>()
            where T : IMessage
        {
            var constructor = typeof(T).GetConstructors().FirstOrDefault(ci =>
            {
                return ci.GetParameters().Any(p => p.ParameterType.Equals(typeof(IServiceProvider)));
            });

            return constructor != null ? (T)constructor.Invoke(new object[] { _provider }) : (T)typeof(T).GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
        }

        /// <inheritdoc />
        public IMessagePublisher CreatePublisher(string topic)
        {
            return new RabbitPublisher(this, _factory.CreateConnection(), topic);
        }

        /// <inheritdoc />
        public IMessagePublisher CreatePublisher(string cluster, string topic)
        {
            return new RabbitHttpPublisher(this, _clusters[cluster], topic);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetClusters() => _clusters.Keys.ToArray();

        /// <inheritdoc />
        public IMessageSubscriber<T> CreateSubscriber<T>(string topic, ushort maxConcurrency)
            where T : IMessage
        {
            return new RabbitSubscriber<T>(this, _factory.CreateConnection(), topic, maxConcurrency, null);
        }

        /// <inheritdoc />
        public IMessageSubscriber<T> CreateSubscriber<T>(string topic, ushort maxConcurrency, string filter)
            where T : IMessage
        {
            return new RabbitSubscriber<T>(this, _factory.CreateConnection(), topic, maxConcurrency, filter);
        }
    }
}