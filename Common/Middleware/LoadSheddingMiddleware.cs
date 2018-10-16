using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;

namespace Lucent.Common.Middleware
{
    /// <summary>
    /// Middleware for implementing load shedding
    /// </summary>
    public sealed class LoadSheddingMiddleware
    {
        readonly RequestDelegate _next;
        readonly ILogger<LoadSheddingMiddleware> _log;
        readonly LoadSheddingConfiguration _config;
        readonly LoadSheddingQueue _queue;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="next">The next delegate in the chain</param>
        /// <param name="log">The log to use for messaging</param>
        /// <param name="options">The available configuration options</param>
        public LoadSheddingMiddleware(RequestDelegate next, ILogger<LoadSheddingMiddleware> log, IOptions<LoadSheddingConfiguration> options)
        {
            _next = next;
            _log = log;
            _config = options == null ? new LoadSheddingConfiguration() : options.Value ?? new LoadSheddingConfiguration();
            _queue = new LoadSheddingQueue(_config.MaxQueueDepth);

            for (var i = 0; i < _config.MaxConcurrentRequests; ++i)
            {
                Task.Factory.StartNew(() =>
                {
                    LoadSheddableItem item;
                    if (_queue.TryRemove(out item, TimeSpan.FromSeconds(60)))
                    {
                        try
                        {
                            item.Source.SetResult(item.HttpContext);
                        }
                        catch (Exception e)
                        {
                            _log.LogError(e, "Failed to execute request");
                        }
                    }
                }, TaskCreationOptions.LongRunning);
            }
        }

        /// <summary>
        /// Invoke the middleware
        /// </summary>
        /// <param name="context">The current HttpContext for the call</param>
        /// <returns>A Task</returns>
        public async Task Invoke(HttpContext context)
        {
            // Create the wrapped item
            var item = new LoadSheddableItem(context)
            {
                StatusCode = _config.StatusCode
            };

            // Try to queue the request
            if (_queue.TryAdd(item, TimeSpan.FromMilliseconds(15)))
            {
                await item.Source.Task.ContinueWith((ctx) =>
                {
                    _next(context);
                }, TaskContinuationOptions.NotOnCanceled);
            }
            else
            {
                // Shed
                context.Response.StatusCode = _config.StatusCode;
            }
        }
    }

    class LoadSheddableItem
    {
        public LoadSheddableItem(HttpContext context)
        {
            Source = new TaskCompletionSource<HttpContext>();
            HttpContext = context;
        }

        public TaskCompletionSource<HttpContext> Source { get; set; }
        public HttpContext HttpContext { get; set; }
        public int StatusCode { get; set; } = 429;
    }

    class LoadSheddingQueue
    {
        static readonly Counter _shedCount = Metrics.CreateCounter("requests_shed", "Number of requests shed", new CounterConfiguration
        {
            LabelNames = new string[] { "method", "path" },
        });

        public const int DEFAULT_CAPACITY = 256;
        public static readonly TimeSpan MAX_WAIT = TimeSpan.FromMinutes(1);

        readonly int BUFFER_MASK = 0xFFF; // Used for bit shifting modulo

        volatile int _head, _tail, _size;
        volatile LoadSheddableItem[] _buffer;
        volatile bool _isOpen, _isClosed;

        int _capacity;
        object _syncLock;

        public LoadSheddingQueue()
            : this(DEFAULT_CAPACITY)
        {
        }

        public LoadSheddingQueue(int capacity)
        {
            if (capacity < 2 || capacity >= Math.Pow(2, 31))
                throw new ArgumentOutOfRangeException("Capacity must be greater than 1 and less than 2^31");

            _syncLock = new object();

            var msb = capacity.MSB() + 1;
            if ((int)Math.Pow(2, msb - 1) == capacity)
                msb--;

            _capacity = (int)Math.Pow(2, msb);

            BUFFER_MASK = 0x0;
            for (var i = 0; i < msb; ++i)
                BUFFER_MASK = (BUFFER_MASK << 1) | 0x1;

            _buffer = new LoadSheddableItem[_capacity];
            _isOpen = false;
            _isClosed = false;

            _head = 0;
            _tail = 0;
            _size = 0;
        }

        /// <summary>
        /// Add a new context to execute
        /// </summary>
        /// <param name="item"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool TryAdd(LoadSheddableItem item, TimeSpan timeout)
        {
            lock (_syncLock)
            {
                // If the queue is full, time to start shedding
                while (Available == 0)
                {
                    var shed = _buffer[_tail];
                    _tail = (_tail + 1) & BUFFER_MASK;
                    _size--;
                    try
                    {
                        shed.HttpContext.Response.StatusCode = shed.StatusCode;
                        shed.Source.SetCanceled();
                    }
                    catch
                    {
                        // swallow errors
                    }
                    finally
                    {
                        _shedCount.WithLabels(shed.HttpContext.Request.Method, shed.HttpContext.Request.Path).Inc();
                    }
                }

                _size++;

                // Increment and wrap the index then assign the item
                _buffer[_head] = item;
                _head = (_head + 1) & BUFFER_MASK;

                // Notify waiting read threads a change has happened
                Monitor.PulseAll(_syncLock);
                return true;
            }
        }

        /// <summary>
        /// Remove the next value if possible
        /// </summary>
        /// <param name="item"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool TryRemove(out LoadSheddableItem item, TimeSpan timeout)
        {
            item = null;

            lock (_syncLock)
            {
                while (Size == 0)
                    if (!Monitor.Wait(_syncLock, timeout))
                        return false; // Failed to synchronize in time

                // Get the item and move the tail
                item = _buffer[_tail];
                _tail = (_tail + 1) & BUFFER_MASK;
                _size--;

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

        public int Count
        {
            get
            {
                lock (_syncLock)
                    return Size;
            }
        }
    }

    /// <summary>
    /// Identifies how the load shedding middleware should handle incoming requests
    /// </summary>
    public enum LoadSheddingStrategy
    {
        /// <summary>
        /// Don't shed anything
        /// </summary>
        None,
        /// <summary>
        /// Tail shedding (queued requests first)
        /// </summary>
        Tail,
        /// <summary>
        /// Head shedding (drop before queue)
        /// </summary>
        Head,
    }

    /// <summary>
    /// Load shedding configuration block
    /// </summary>
    public class LoadSheddingConfiguration
    {
        /// <summary>
        /// Choose the strategy
        /// </summary>
        public LoadSheddingStrategy Strategy { get; set; }

        /// <summary>
        /// Sets the target latency maximum
        /// </summary>
        public int MaxLatencyMS { get; set; } = 1000;

        /// <summary>
        /// Sets the maximum number of concurrent requests
        /// </summary>
        public int MaxConcurrentRequests { get; set; }

        /// <summary>
        /// Sets the maximum number of requests to queue at any time
        /// </summary>
        public int MaxQueueDepth { get; set; }

        /// <summary>
        /// Sets the default status code for requests that are shed
        /// </summary>
        public int StatusCode { get; set; } = 429;

        /// <summary>
        /// Indicates if the load shedding can manipulate values to maintain latency targets
        /// </summary>
        public bool IsAdaptive { get; set; }
    }
}