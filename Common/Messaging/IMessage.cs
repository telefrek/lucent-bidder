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
        /// Gets/Sets the current message state
        /// </summary>
        MessageState State {get;set;}

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