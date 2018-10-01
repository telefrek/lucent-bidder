using System.Collections.Generic;

namespace Lucent.Common.Messaging
{
    /// <summary>
    /// A message abstraction interface
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// Gets/Sets the message identifier
        /// </summary>
        string MessageId { get; set; }

        /// <summary>
        /// The message Route
        /// </summary>
        string Route { get; set; }

        /// <summary>
        /// Gets/Sets the current message state
        /// </summary>
        MessageState State { get; set; }

        /// <summary>
        /// Custom defined headers
        /// </summary> 
        IDictionary<string, object> Headers { get; set; }

        /// <summary>
        /// The message timestamp when it was created
        /// </summary>
        long Timestamp { get; set; }

        /// <summary>
        /// The message correlation id
        /// </summary>
        string CorrelationId { get; set; }

        /// <summary>
        /// Flag to track if this is the first delivery of the message
        /// </summary>
        bool FirstDelivery { get; set; }

        /// <summary>
        /// Tracks the content type of the contents
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// Transforms the message into a byte array for transport
        /// </summary>
        /// <returns>A byte array representing the message</returns>
        byte[] ToBytes();

        /// <summary>
        /// Loads a message from the given byte array
        /// </summary>
        /// <param name="buffer">The buffer with the message contents</param>
        void Load(byte[] buffer);
    }
}