using System;

namespace Lucent.Common.Messaging
{
    /// <summary>
    /// Abstract publisher for messages
    /// </summary>
    public interface IMessagePublisher : IDisposable
    {
        bool TryPublish(IMessage message);
    }
}