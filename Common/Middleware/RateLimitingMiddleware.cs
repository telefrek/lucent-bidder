using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Middleware
{

    /// <summary>
    /// Middleware for implementing load shedding
    /// </summary>
    public sealed class RateLimitingMiddleware
    {
        readonly RequestDelegate _next;
        readonly ILogger<RateLimitingMiddleware> _log;
        long _curBids = 0;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _log = logger;
        }

        /// <summary>
        /// Invoke wrapper
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                if (Interlocked.Increment(ref _curBids) < 64)
                    await _next(context);
                else
                    context.Response.StatusCode = 204;
            }
            finally
            {
                Interlocked.Decrement(ref _curBids);
            }
        }
    }
}