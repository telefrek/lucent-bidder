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

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="options">Configuration parameters for the environment</param>
        public RabbitFactory(IOptions<RabbitConfiguration> options)
        {
            var configuration = options.Value;

            _factory = new ConnectionFactory();
            _factory.HostName = configuration.Host;
            _factory.UserName = configuration.User;
            _factory.Password = configuration.Credentials;
        }

        /// <summary>
        /// Creates a publisher for the given topic
        /// </summary>
        /// <param name="topic">The publisher topic</param>
        /// <returns>An instantiated publisher</returns>
        public IMessagePublisher CreatePublisher(string topic)
        {
            return new RabbitPublisher(_factory.CreateConnection(), topic);
        }

        /// <summary>
        /// Creates a subscriber for the given topic
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="maxConcurrency"></param>
        /// <returns></returns>
        public IMessageSubscriber<T> CreateSubscriber<T>(string topic, ushort maxConcurrency)
            where T : IMessage, new()
        {
            return new RabbitSubscriber<T>(_factory.CreateConnection(), topic, maxConcurrency, null);
        }


        /// <summary>
        /// Creates a subscriber for the given topic
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="maxConcurrency"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IMessageSubscriber<T> CreateSubscriber<T>(string topic, ushort maxConcurrency, string filter)
            where T : IMessage, new()
        {
            return new RabbitSubscriber<T>(_factory.CreateConnection(), topic, maxConcurrency, filter);
        }
    }
}