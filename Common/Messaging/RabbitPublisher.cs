using RabbitMQ.Client;

namespace Lucent.Common.Messaging
{
    internal class RabbitPublisher : IMessagePublisher
    {
        readonly IConnection _conn;
        readonly IModel _channel;

        public string Topic {get;set;}

        public RabbitPublisher(IConnection conn, string topic)
        {
            _conn = conn;
            _channel = conn.CreateModel();
            _channel.ExchangeDeclare(topic, "fanout");
            Topic = topic;
        }

        public bool TryPublish(IMessage message)
        {
            _channel.BasicPublish(exchange: Topic,
                     routingKey: "",
                     basicProperties: null,
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