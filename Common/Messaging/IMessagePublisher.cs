using System;

namespace Lucent.Common.Messaging
{
    /// <summary>
    /// Abstract publisher for messages
    /// </summary>
    public interface IMessagePublisher : IDisposable
    {
        /// <summary>
        /// Gets this publisher topic
        /// </summary>
        string Topic { get; }

        /// <summary>
        /// Attempts to publish the message for this topic
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool TryPublish(IMessage message);
    }
}