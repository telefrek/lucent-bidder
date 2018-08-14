namespace Lucent.Common.Messaging
{
    /// <summary>
    /// Factory for creating publishers and subscribers
    /// </summary>
    public interface IMessageFactory
    {
        /// <summary>
        /// Creates a new publisher for the given topic
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        IMessagePublisher CreatePublisher(string topic);

        /// <summary>
        /// Creates a new subscriber for the given topic
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="maxConcurrency"></param>
        /// <returns></returns>
        IMessageSubscriber<T> CreateSubscriber<T>(string topic, ushort maxConcurrency) where T : IMessage, new();
    }
}