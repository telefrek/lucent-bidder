using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.Messaging
{
    /// <summary>
    /// In memory messaging implementation
    /// </summary>
    public class InMemoryMessageFactory : IMessageFactory
    {
        ISerializationContext _serializationContext;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="serializationContext"></param>
        public InMemoryMessageFactory(ISerializationContext serializationContext)
        {
            _serializationContext = serializationContext;
        }

        static Dictionary<string, InMemoryQueue> _queues = new Dictionary<string, InMemoryQueue>();

        /// <inheritdoc/>
        public string WildcardFilter => "*";

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
            if (!_queues.ContainsKey(topic))
            {
                _queues.Add(topic, new InMemoryQueue { Subscribers = new List<object>() });
                _queues[topic].Publisher = new InMemoryPublisher(_queues[topic]);
            }

            return _queues[topic].Publisher;
        }

        /// <inheritdoc />
        public IMessagePublisher CreatePublisher(string cluster, string topic)
        {
            if (!_queues.ContainsKey(topic))
            {
                _queues.Add(topic, new InMemoryQueue { Subscribers = new List<object>() });
                _queues[topic].Publisher = new InMemoryPublisher(_queues[topic]);
            }

            return _queues[topic].Publisher;
        }

        /// <inheritdoc />
        public IMessageSubscriber<T> CreateSubscriber<T>(string topic) where T : IMessage
        {
            if (!_queues.ContainsKey(topic))
            {
                _queues.Add(topic, new InMemoryQueue { Subscribers = new List<object>() });
                _queues[topic].Publisher = new InMemoryPublisher(_queues[topic]);
            }

            var sub = new InMemorySubscriber<T> { Topic = topic };
            _queues[topic].Subscribers.Add(sub);

            return sub;
        }

        /// <inheritdoc />
        public IMessageSubscriber<T> CreateSubscriber<T>(string topic, string filter) where T : IMessage
        {
            if (!_queues.ContainsKey(topic))
            {
                _queues.Add(topic, new InMemoryQueue { Subscribers = new List<object>() });
                _queues[topic].Publisher = new InMemoryPublisher(_queues[topic]);
            }

            var sub = new InMemorySubscriber<T> { Topic = topic, Filter = filter };
            _queues[topic].Subscribers.Add(sub);

            return sub;
        }

        /// <inheritdoc />
        public IEnumerable<string> GetClusters() => new List<string>();

        class InMemoryQueue
        {
            public bool IsBroadcast { get; set; } = true;
            public IMessagePublisher Publisher { get; set; }
            public List<object> Subscribers { get; set; }
        }

        class InMemorySubscriber<T> : IMessageSubscriber<T>
            where T : IMessage
        {
            public string Topic { get; set; }

            public string Filter { get; set; }

            public Func<T, Task> OnReceive { get; set; }

            public void Dispose()
            {
            }
        }
        class InMemoryPublisher : IMessagePublisher
        {
            InMemoryQueue _queue;

            public InMemoryPublisher(InMemoryQueue queue)
            {
                _queue = queue;
            }

            public string Topic { get; set; }

            public void Dispose()
            {
            }

            public async Task<bool> TryBroadcast(IMessage message) => await TryPublish(message);

            public async Task<bool> TryPublish(IMessage message)
            {
                foreach (dynamic sub in _queue.Subscribers.ToList())
                {
                    if (sub.Filter != null && message.Route != null && !Regex.IsMatch(message.Route, "^" + Regex.Escape(sub.Filter.Replace("#", "*")).Replace("\\?", ".").Replace("\\*", ".*") + "$"))
                        continue;

                    else if (sub.OnReceive != null)
                    {
                        if (sub.OnReceive.Method.GetParameters()[0].ParameterType.IsAssignableFrom(message.GetType()))
                            sub.OnReceive.Method.Invoke(sub.OnReceive.Target, new object[] { message });
                    }
                }

                return await Task.FromResult(true);
            }
        }
    }
}