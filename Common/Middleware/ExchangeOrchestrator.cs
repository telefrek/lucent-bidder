using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucent.Common;
using Lucent.Common.Entities;
using Lucent.Common.Entities.Events;
using Lucent.Common.Events;
using Lucent.Common.Messaging;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Lucent.Common.Middleware
{
    /// <summary>
    /// Handle Exxchange API management
    /// </summary>
    public class ExchangeOrchestrator : EntityRestApi<Exchange>
    {
        /// <inheritdoc/>
        public ExchangeOrchestrator(RequestDelegate next, IStorageManager storageManager, IMessageFactory messageFactory, ISerializationContext serializationContext, ILogger<EntityRestApi<Exchange>> logger) : base(next, storageManager, messageFactory, serializationContext, logger)
        {
        }

        /// <inheritdoc/>
        protected async override Task<Exchange> ReadEntity(HttpContext httpContext)
        {
            Exchange exchange = null;

            if (!httpContext.Request.ContentType.IsNullOrDefault() && httpContext.Request.ContentType.Contains("multipart"))
            {
                // /api/exchanges/id
                exchange = await _entityRepository.Get(new GuidStorageKey(Guid.Parse(httpContext.Request.Path.Value.Split("/").Last())));
                if (exchange != null)
                {
                    var header = MediaTypeHeaderValue.Parse(httpContext.Request.ContentType);
                    var boundary = HeaderUtilities.RemoveQuotes(header.Boundary).Value;
                    var reader = new MultipartReader(boundary, httpContext.Request.Body);

                    var section = await reader.ReadNextSectionAsync();
                    if (section != null)
                    {
                        ContentDispositionHeaderValue contentDisposition;
                        if (ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition))
                        {
                            exchange.Code = new MemoryStream();
                            await httpContext.Request.Body.CopyToAsync(exchange.Code);
                            exchange.Code.Seek(0, SeekOrigin.Begin);
                            exchange.LastCodeUpdate = DateTime.UtcNow;
                        }
                    }
                }
            }
            else
                return await base.ReadEntity(httpContext);

            return exchange;
        }
    }
}