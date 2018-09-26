using RabbitMQ.Client;

namespace Lucent.Common.Messaging
{
    internal class RabbitPublisher : IMessagePublisher
    {
        readonly IConnection _conn;
        readonly IModel _channel;
        readonly IMessageFactory _factory;

        public string Topic { get; set; }

        public RabbitPublisher(IMessageFactory factory, IConnection conn, string topic)
        {
            _conn = conn;
            _factory = factory;
            _channel = conn.CreateModel();
            _channel.ExchangeDeclare(topic, "topic");
            Topic = topic;
        }

        public bool TryPublish(IMessage message)
        {
            var props = _channel.CreateBasicProperties();
            if (!string.IsNullOrEmpty(message.CorrelationId))
                props.CorrelationId = message.CorrelationId;

            _channel.BasicPublish(exchange: Topic,
                     routingKey: message.Route ?? "undefined",
                     basicProperties: props,
                     body: message.ToBytes());

            return true;
        }

        public void Dispose()
        {
            _channel.Close();
            _conn.Close();
        }
    }
}