using System;

namespace Lucent.Common.Messaging
{
    // Want the client to subscribe and be able to control concurrent executions.
    public interface IMessageSubscriber : IDisposable
    {
        Action<IMessage> OnReceive { get; set; }
    }
}