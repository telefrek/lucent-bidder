using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.Exchanges;
using Lucent.Common.Messaging;
using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Lucent.Common.Middleware
{
    /// <summary>
    /// Middleware for controlling postback notifications
    /// </summary>
    public class PostbackMiddleware
    {
        ILogger<PostbackMiddleware> _log;
        ISerializationContext _serializationContext;
        IStorageManager _storageManager;

        int _next = 0;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="log"></param>
        /// <param name="factory"></param>
        /// <param name="serializationContext"></param>
        /// <param name="storageManager"></param>
        public PostbackMiddleware(RequestDelegate next, ILogger<PostbackMiddleware> log, IMessageFactory factory, ISerializationContext serializationContext, IStorageManager storageManager)
        {
            _log = log;
            _serializationContext = serializationContext;
            _storageManager = storageManager;
        }

        /// <summary>
        /// Handle the request asynchronously
        /// </summary>
        /// <param name="context">The current http context</param>
        /// <returns>A completed pipeline step</returns>
        public async Task HandleAsync(HttpContext context)
        {
            // Check type of postback
            var query = context.Request.Query;

            try
            {
                if (query.ContainsKey("lbrdl"))
                {
                    context.Response.Redirect(query["lbrdl"].First().SafeBase64Decode());
                    return;
                }

                if (query.ContainsKey("lbctx"))
                {
                    var lucentContext = query["lbctx"].First().SafeBase64Decode();
                    var campaignId = lucentContext.Substring(0, 22).DecodeGuid().ToString();
                    var camp = await _storageManager.GetRepository<Campaign>().Get(campaignId);

                    if (camp != null)
                    {
                        context.Response.StatusCode = StatusCodes.Status202Accepted;
                        return;
                    }
                }

                context.Response.StatusCode = StatusCodes.Status200OK;
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to handle postback");
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
    }
}