using System;
using System.Threading.Tasks;

namespace Lucent.Common.Messaging
{
    /// <summary>
    /// Abstract message subscriber interface
    /// </summary>
    public interface IMessageSubscriber<T> : IDisposable
        where T : IMessage
    {
        /// <summary>
        /// Gets the topic this subscriber listens to
        /// </summary>
        string Topic { get; }

        /// <summary>
        /// Gets/Sets the action this subscriber should perform when a message is received
        /// </summary>
        /// <value></value>
        Func<T, Task> OnReceive { get; set; }
    }
}