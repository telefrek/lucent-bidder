using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Lucent.Common.Messaging
{
    public class RabbitFactory
    {
        public static RabbitFactory Instance = new RabbitFactory();

        private ConnectionFactory _factory;

        protected RabbitFactory()
        {
            _factory = new ConnectionFactory();
            _factory.HostName = "localhost";
            _factory.UserName = "test";
            _factory.Password = "test";
        }

        public IMessagePublisher GetPublisher(string topic)
        {
            return new RabbitPublisher(_factory.CreateConnection());
        }

        public IMessageSubscriber GetSubscriber(string topic, string subscriberId)
        {
            return new RabbitSubscriber(_factory.CreateConnection());
        }
    }

    public class RabbitPublisher : IMessagePublisher
    {
        readonly IConnection _conn;
        readonly IModel _channel;

        public RabbitPublisher(IConnection conn)
        {
            _conn = conn;
            _channel = conn.CreateModel();
            _channel.ExchangeDeclare("test", "fanout");
        }

        public bool TryPublish(IMessage message)
        {
            _channel.BasicPublish(exchange: "test",
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

    public class RabbitSubscriber : IMessageSubscriber
    {
        readonly IConnection _conn;
        readonly IModel _channel;
        readonly EventingBasicConsumer _consumer;
        readonly string _queueName;
        readonly Queue<string> _messages = new Queue<string>();

        public RabbitSubscriber(IConnection conn)
        {
            _conn = conn;
            _channel = conn.CreateModel();
            _channel.ExchangeDeclare(exchange: "test", type: "fanout");
            _queueName = _channel.QueueDeclare().QueueName;

            _channel.QueueBind(queue: _queueName,
                  exchange: "test",
                  routingKey: "");

            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += (model, ea) =>
            {
                if(OnReceive != null)
                {
                    using(var ms = new MemoryStream(ea.Body))
                    {
                        try
                        {

                        }
                        catch
                        {
                            // Log this later
                        }
                    }

                }
            };

            _channel.BasicConsume(queue: _queueName,
                                 autoAck: true,
                                 consumer: _consumer);
        }

        public Action<IMessage> OnReceive { get; set; }

        public bool TryReceive(out string obj)
        {
            obj = null;

            if (_messages.Count > 0)
                obj = _messages.Dequeue();

            return obj != null;
        }
        public void Dispose()
        {
            _channel.Close();
            _conn.Close();
        }
    }
}