using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        readonly LoadSheddingLock _lock;

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
            _lock = new LoadSheddingLock(Math.Max(1, _config.MaxQueueDepth), Math.Max(1, _config.MaxConcurrentRequests), _config.Strategy, _log);
        }

        /// <summary>
        /// Invoke the middleware
        /// </summary>
        /// <param name="context">The current HttpContext for the call</param>
        /// <returns>A Task</returns>
        public async Task Invoke(HttpContext context)
        {
            // Start timing for tracking latency, etc.
            var sw = Stopwatch.StartNew();

            // Try to queue the request
            if (await _lock.TryAcquire())
            {
                try
                {
                    // Invoke the next task
                    await _next.Invoke(context);
                }
                finally
                {
                    _lock.Release();
                }
            }
            else
            {
                context.Response.StatusCode = _config.StatusCode;
            }

            // Done processing, track stats
            var elapsed = sw.ElapsedMilliseconds;
        }
    }

    /// <summary>
    /// Lock that sheds load
    /// </summary>
    public class LoadSheddingLock
    {
        readonly Queue<TaskCompletionSource<bool>> _backlog;
        volatile int _running, _queued;
        readonly int _maxSize, _maxConcurrency, _maxWait;
        readonly object _syncLock = new object();
        readonly LoadSheddingStrategy _strategy;
        readonly ILogger _log;

        public LoadSheddingLock(int maxItems, int maxConcurrent, LoadSheddingStrategy strategy, ILogger log)
        {
            _backlog = new Queue<TaskCompletionSource<bool>>();
            _running = 0;
            _queued = 0;
            _maxSize = maxItems;
            _maxConcurrency = maxConcurrent;
            _strategy = strategy;
            _log = log;
        }

        /// <summary>
        /// Tries to asynchronously acquire a lock
        /// </summary>
        /// <returns>A task that will complete when the lock is available, or rejected</returns>
        public Task<bool> TryAcquire()
        {
            var tcs = new TaskCompletionSource<bool>();

            lock(_syncLock)
            {
                if(_running < _maxConcurrency)
                {
                    _running++;
                    tcs.SetResult(true);
                }
                else if(_backlog.Count < _maxSize)
                {
                    _backlog.Enqueue(tcs);
                }
                else
                {
                    _log.LogWarning("Shedding load : ({0},{1})", _running, _backlog.Count);
                    if(_strategy == LoadSheddingStrategy.Head)
                    {
                        var next = _backlog.Dequeue();
                        next.SetResult(false);
                        _backlog.Enqueue(tcs);
                    }
                    else
                    {
                        tcs.SetResult(false);
                    }
                }
            }

            return tcs.Task;
        }

        public void Release()
        {
            lock(_syncLock)
            {
                if(_backlog.Count > 0)
                {
                    var next = _backlog.Dequeue();
                    next.SetResult(true);
                }
                else
                {
                    _running--;
                }
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