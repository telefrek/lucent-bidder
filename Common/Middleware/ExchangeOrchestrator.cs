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

                _log.LogInformation("Getting exchange to add code to");
                var segments = httpContext.Request.Path.Value.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                exchange = new Exchange();
                exchange.Key.Parse(segments.Last());
                exchange = await _entityRepository.Get(exchange.Key);

                if (exchange != null)
                {
                    _log.LogInformation("Reading section");
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
                            await section.Body.CopyToAsync(exchange.Code);
                            exchange.Code.Seek(0, SeekOrigin.Begin);
                            _log.LogInformation("Loaded {0} bytes", exchange.Code.Length);
                            exchange.LastCodeUpdate = DateTime.UtcNow;
                        }
                    }
                    else
                        _log.LogWarning("No section found for code");
                }
                else
                    _log.LogWarning("No exchange found for code");
            }
            else
                return await base.ReadEntity(httpContext);

            return exchange;
        }
    }
}