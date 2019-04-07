using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Lucent.Common.Collections
{
    /// <summary>
    /// Provides a buffered producer/consumer buffer backed by a circular array.
    /// </summary>
    /// <typeparam name="T">The type of object provided by the ring buffer.</typeparam>
    public class RingBuffer<T> : IRingBuffer<T>
    {
        /// <summary>
        /// Default capacity
        /// </summary>
        public const int DEFAULT_CAPACITY = 4096;

        /// <summary>
        /// Default timeout
        /// </summary>
        /// <returns></returns>
        public static readonly TimeSpan MAX_WAIT = TimeSpan.FromMinutes(1);

        readonly int BUFFER_MASK = 0xFFF; // Used for bit shifting modulo

        volatile int _head, _tail, _size;
        volatile T[] _buffer;
        volatile bool _isOpen, _isClosed;

        int _capacity;
        object _syncLock;

        /// <summary>
        /// Default constructor
        /// </summary>
        public RingBuffer()
            : this(DEFAULT_CAPACITY)
        {
        }

        /// <summary>
        /// Capacity constructor
        /// </summary>
        /// <param name="capacity"></param>
        public RingBuffer(int capacity)
        {
            if (capacity < 2 || capacity >= Math.Pow(2, 31))
                throw new ArgumentOutOfRangeException("Capacity must be greater than 1 and less than 2^31");

            _syncLock = new object();

            var msb = capacity.MSB() + 1;
            _capacity = (int)Math.Pow(2, msb);

            BUFFER_MASK = 0x0;
            for (var i = 0; i < msb; ++i)
                BUFFER_MASK = (BUFFER_MASK << 1) | 0x1;

            _buffer = new T[_capacity];
            _isOpen = false;
            _isClosed = false;

            _head = 0;
            _tail = 0;
            _size = 0;
        }

        bool _TryAdd(T item, TimeSpan timeout)
        {
            lock (_syncLock)
            {
                // Wait while the buffer is full and not closed
                while (!_isClosed && Available == 0)
                    if (!Monitor.Wait(_syncLock, timeout))
                        return false; // Failed to synchronize in the given time

                // Reject adding to a closed buffer
                if (_isClosed)
                    return false;

                _size++;

                // Increment and wrap the index then assign the item
                _buffer[_head] = item;
                _head = (_head + 1) & BUFFER_MASK;

                // Notify waiting read threads a change has happened
                Monitor.PulseAll(_syncLock);
                return true;
            }
        }

        bool _TryAdd(T[] items, TimeSpan timeout)
        {
            lock (_syncLock)
            {
                // Wait for the appropriate capacity to become available or the buffer to close
                do
                {
                    // Calculate the current empty capacity and wait for it to reach the item size
                    if (Available >= items.Length)
                        break;

                    if (!Monitor.Wait(_syncLock, timeout))
                        return false; // Failed to synchronzed in the given time
                }
                while (!_isClosed);

                // Reject adding to a closed buffer
                if (_isClosed)
                    return false;

                // Copy the array contents
                var l = items.Length;
                for (var i = 0; i < l; ++i)
                {
                    _buffer[_head] = items[i];
                    _head = (_head + 1) & BUFFER_MASK;
                }

                _size += l;

                // Notify waiting read threads a change has happened
                Monitor.PulseAll(_syncLock);

                return true;
            }
        }

        bool _TryRemove(out T item, TimeSpan timeout)
        {
            item = default(T);

            lock (_syncLock)
            {
                while (!_isClosed && Size == 0)
                    if (!Monitor.Wait(_syncLock, timeout))
                        return false; // Failed to synchronize in time

                // Can't return a new value from a closed buffer with no publishing
                if (_isClosed && Size == 0)
                    return false;

                // Get the item and move the tail
                item = _buffer[_tail];
                _tail = (_tail + 1) & BUFFER_MASK;
                _size--;

                // Notify that something was consumed
                Monitor.PulseAll(_syncLock);

                return true;
            }
        }

        bool _TryRemove(out T[] items, TimeSpan timeout)
        {
            items = new T[0];

            lock (_syncLock)
            {
                while (!_isClosed && Size == 0)
                    if (!Monitor.Wait(_syncLock, timeout))
                        return false; // Failed to synchronize in time

                // Can't return a new value from a closed buffer with no publishing
                if (_isClosed && Size == 0)
                    return false;

                // Resize the array
                var l = Size;
                items = new T[l];

                // Keep adding items until the appropriate size has been read
                for (var i = 0; i < l; ++i)
                {
                    // Get the item and move the tail
                    items[i] = _buffer[_tail];
                    _tail = (_tail + 1) & BUFFER_MASK;
                }

                _size -= l;

                // Notify that something was consumed
                Monitor.PulseAll(_syncLock);

                return true;
            }
        }

        bool _TryRemove(out T[] items, int maxItems, TimeSpan timeout)
        {
            items = new T[0];

            lock (_syncLock)
            {
                while (!_isClosed && Size == 0)
                    if (!Monitor.Wait(_syncLock, timeout))
                        return false; // Failed to synchronize in time

                // Can't return a new value from a closed buffer with no publishing
                if (_isClosed && Size == 0)
                    return false;

                // Resize the array
                var l = Math.Min(Size, maxItems);
                items = new T[l];

                // Keep adding items until the appropriate size has been read
                for (var i = 0; i < l; ++i)
                {
                    // Get the item and move the tail
                    items[i] = _buffer[_tail];
                    _tail = (_tail + 1) & BUFFER_MASK;
                }

                _size -= l;

                // Notify that something was consumed
                Monitor.PulseAll(_syncLock);

                return true;
            }
        }

        int Size
        {
            get
            {
                return _size;
            }
        }

        int Available
        {
            get
            {
                return _capacity - _size - 1;
            }
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            Close();
        }

        #endregion

        #region ICollection<T> Members

        /// <inheritdoc/>
        public void Add(T item)
        {
            // Don't add to a closed buffer
            if (_isClosed)
                throw new InvalidOperationException("Cannot add to a closed buffer");

            // Try to add the item and throw an exception if it fails with infinite timeout
            if (!TryAdd(item, MAX_WAIT))
                throw new InvalidOperationException("Buffer is unavailable or closed");
        }

        /// <inheritdoc/>
        public void Clear()
        {
            throw new NotSupportedException("Clearing a ring buffer is not supported");
        }

        /// <inheritdoc/>
        public bool Contains(T item)
        {
            throw new NotSupportedException("Searching a ring buffer is not currently supported");
        }

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotSupportedException("Please use the TryRemove method on the RingBuffer instead");
        }

        /// <inheritdoc/>
        public int Count
        {
            get
            {
                lock (_syncLock)
                    return Size;
            }
        }

        /// <inheritdoc/>
        public bool IsReadOnly
        {
            get { return _isClosed; }
        }

        /// <inheritdoc/>
        public bool Remove(T item)
        {
            throw new NotSupportedException("Removal of items from the ring buffer is not supported");
        }

        #endregion

        #region IEnumerable<T> Members

        /// <inheritdoc/>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new RingBufferEnumerator<T>(this);
        }

        #endregion

        #region IEnumerable Members

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new RingBufferEnumerator<T>(this);
        }

        #endregion

        #region IRingBuffer<T> Members

        /// <inheritdoc/>
        public void Open()
        {
            if (!_isClosed)
                _isOpen = true;
        }

        /// <inheritdoc/>
        public void Close()
        {
            if (_isOpen)
                _isClosed = true;

            lock (_syncLock)
                Monitor.PulseAll(_syncLock);
        }

        /// <inheritdoc/>
        public bool IsComplete { get { return _isClosed; } }

        /// <inheritdoc/>
        public bool TryAdd(T instance)
        {
            if (!_isOpen)
                throw new InvalidOperationException("Buffer must be opened before writing");
            return _TryAdd(instance, MAX_WAIT);
        }

        /// <inheritdoc/>
        public bool TryAdd(T instance, TimeSpan timeout)
        {
            if (!_isOpen)
                throw new InvalidOperationException("Buffer must be opened before writing");
            if (timeout < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("Timeout cannot be less than Zero!");

            return _TryAdd(instance, timeout);
        }

        /// <inheritdoc/>
        public bool TryAddRange(T[] instances)
        {
            if (!_isOpen)
                throw new InvalidOperationException("Buffer must be opened before writing");
            if (instances.Length >= _capacity)
                throw new ArgumentOutOfRangeException("Instances must be smaller than the buffer capacity");

            return _TryAdd(instances, MAX_WAIT);
        }

        /// <inheritdoc/>
        public bool TryAddRange(T[] instances, TimeSpan timeout)
        {
            if (!_isOpen)
                throw new InvalidOperationException("Buffer must be opened before writing");
            if (instances.Length >= _capacity)
                throw new ArgumentOutOfRangeException("Instances must be smaller than the buffer capacity");
            if (timeout < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("Timeout cannot be less than Zero!");

            return _TryAdd(instances, timeout);
        }

        /// <inheritdoc/>
        public bool TryRemove(out T instance)
        {
            if (!_isOpen)
                throw new InvalidOperationException("Buffer must be opened before reading");

            return _TryRemove(out instance, MAX_WAIT);
        }

        /// <inheritdoc/>
        public bool TryRemove(out T instance, TimeSpan timeout)
        {
            if (!_isOpen)
                throw new InvalidOperationException("Buffer must be opened before reading");

            if (timeout < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("Timeout cannot be less than Zero!");

            return _TryRemove(out instance, timeout);
        }

        /// <inheritdoc/>
        public bool TryRemoveRange(out T[] instances)
        {
            if (!_isOpen)
                throw new InvalidOperationException("Buffer must be opened before reading");

            return _TryRemove(out instances, MAX_WAIT);
        }

        /// <inheritdoc/>
        public bool TryRemoveRange(out T[] instances, TimeSpan timeout)
        {
            if (!_isOpen)
                throw new InvalidOperationException("Buffer must be opened before reading");
            if (timeout < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("Timeout cannot be less than Zero!");

            return _TryRemove(out instances, timeout);
        }

        /// <inheritdoc/>
        public bool TryRemoveRange(out T[] instances, int maxItems)
        {
            if (!_isOpen)
                throw new InvalidOperationException("Buffer must be opened before reading");

            return _TryRemove(out instances, maxItems, MAX_WAIT);
        }

        /// <inheritdoc/>
        public bool TryRemoveRange(out T[] instances, int maxItems, TimeSpan timeout)
        {
            if (!_isOpen)
                throw new InvalidOperationException("Buffer must be opened before reading");

            return _TryRemove(out instances, maxItems, timeout);
        }

        #endregion
    }
}
