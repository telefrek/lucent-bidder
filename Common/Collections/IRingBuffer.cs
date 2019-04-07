using System;
using System.Collections;
using System.Collections.Generic;

namespace Lucent.Common.Collections
{
    /// <summary>
    /// Circular buffer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRingBuffer<T> : IEnumerable, IEnumerable<T>, ICollection<T>, IDisposable
    {
        /// <summary>
        /// Open the buffer
        /// </summary>
        void Open();

        /// <summary>
        /// Close the buffer
        /// </summary>
        void Close();

        /// <summary>
        /// Check if the buffer is completely consumed
        /// </summary>
        /// <value></value>
        bool IsComplete { get; }

        /// <summary>
        /// Try to add an instance (blocking    )
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        bool TryAdd(T instance);

        /// <summary>
        /// Try to add an instance within the timeout
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        bool TryAdd(T instance, TimeSpan timeout);

        /// <summary>
        /// Try to add a range of objects (blocking)
        /// </summary>
        /// <param name="instances"></param>
        /// <returns></returns>
        bool TryAddRange(T[] instances);

        /// <summary>
        /// Try to add a range of objects within the timeout
        /// </summary>
        /// <param name="instances"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        bool TryAddRange(T[] instances, TimeSpan timeout);

        /// <summary>
        /// Try to get the next object (blocking)
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        bool TryRemove(out T instance);

        /// <summary>
        /// Try to get the next object within the timeout
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        bool TryRemove(out T instance, TimeSpan timeout);

        /// <summary>
        /// Try to remove a range of objects (blocking)
        /// </summary>
        /// <param name="instances"></param>
        /// <returns></returns>
        bool TryRemoveRange(out T[] instances);

        /// <summary>
        /// Try to remove a range of objects with the timeout
        /// </summary>
        /// <param name="instances"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        bool TryRemoveRange(out T[] instances, TimeSpan timeout);

        /// <summary>
        /// Try to remove up to maxItems from the buffer (blocking)
        /// </summary>
        /// <param name="instances"></param>
        /// <param name="maxItems"></param>
        /// <returns></returns>
        bool TryRemoveRange(out T[] instances, int maxItems);

        /// <summary>
        /// Try to remove up to maxItems from the buffer within the timeout
        /// </summary>
        /// <param name="instances"></param>
        /// <param name="maxItems"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        bool TryRemoveRange( out T[] instances, int maxItems, TimeSpan timeout);
    }
}
