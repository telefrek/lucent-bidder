using System;
using System.Collections;
using System.Collections.Generic;

namespace Lucent.Common.Collections
{
    /// <summary>
    /// Enumerates over a collection of objects contained in a ring buffer
    /// </summary>
    /// <typeparam name="T">The type of object exposed in the buffer.</typeparam>
    public sealed class RingBufferEnumerator<T> : IEnumerator<T>
    {
        IRingBuffer<T> _buffer;
        T[] _currentBlock;
        int _ix;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="buffer"></param>
        public RingBufferEnumerator(IRingBuffer<T> buffer)
        {
            _buffer = buffer;
            _currentBlock = new T[0];
            _ix = 0;
        }

        #region IEnumerator<T> Members

        /// <inheritdoc/>
        T IEnumerator<T>.Current
        {
            get { return _currentBlock[_ix++]; }
        }

        #endregion

        #region IDisposable Members

        /// <inheritdoc/>
        void IDisposable.Dispose()
        {
            // Nothing to do here
        }

        #endregion

        #region IEnumerator Members

        /// <inheritdoc/>
        object IEnumerator.Current
        {
            get { return _currentBlock[_ix++]; }
        }

        /// <inheritdoc/>
        bool IEnumerator.MoveNext()
        {
            // There is more in the local buffer
            if (_ix < _currentBlock.Length)
                return true;

            // Reset the internal index
            _ix = 0;

            // If it's not complete, block the wait for a publisher to send one or more items
            if (!_buffer.IsComplete)
                return _buffer.TryRemoveRange(out _currentBlock);

            // If the count is 0 and the buffer is completed, no other messages will come
            if (_buffer.Count == 0)
                return false;

            // Block the calling thread and try to remove the next chunk
            return _buffer.TryRemoveRange(out _currentBlock);
        }

        /// <inheritdoc/>
        void IEnumerator.Reset()
        {
            throw new NotSupportedException("Ring Buffers cannot be reset.");
        }

        #endregion
    }
}
