using System.Collections.Generic;

namespace Lucent.Common.Messaging
{
    /// <summary>
    /// Factory for creating publishers and subscribers
    /// </summary>
    public interface IMessageFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T CreateMessage<T>()
        where T: IMessage;

        /// <summary>
        /// Creates a new publisher for the given topic
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        IMessagePublisher CreatePublisher(string topic);

        /// <summary>
        /// Creates a new publisher for the given topic
        /// </summary>
        /// <param name="cluster"></param>
        /// <param name="topic"></param>
        /// <returns></returns>
        IMessagePublisher CreatePublisher(string cluster, string topic);

        /// <summary>
        /// Return the set of clusters this server knows about
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetClusters();

        /// <summary>
        /// Creates a new subscriber for the given topic
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="maxConcurrency"></param>
        /// <returns></returns>
        IMessageSubscriber<T> CreateSubscriber<T>(string topic, ushort maxConcurrency) where T : IMessage;

        /// <summary>
        /// Creates a new subscriber for the given topic
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="maxConcurrency"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        IMessageSubscriber<T> CreateSubscriber<T>(string topic, ushort maxConcurrency, string filter) where T : IMessage;
    }
}