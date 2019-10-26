using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lucent.Common.Caching;
using Lucent.Common.Entities;
using Lucent.Common.Media;
using Lucent.Common.Messaging;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Lucent.Common.Middleware
{
    /// <summary>
    /// 
    /// </summary>
    public class CreativeApi : EntityRestApi<Creative>
    {
        IMediaScanner _mediaScanner;
        IConfiguration _config;

        /// <inheritdoc/>
        public CreativeApi(RequestDelegate next, StorageCache storageCache, IStorageManager storageManager, IMessageFactory messageFactory, ISerializationContext serializationContext, ILogger<EntityRestApi<Creative>> logger, IMediaScanner mediaScanner,
        IConfiguration configuration) : base(next, storageCache, storageManager, messageFactory, serializationContext, logger, null)
        {
            _mediaScanner = mediaScanner;
            _config = configuration;
        }

        /// <inheritdoc/>
        public override async Task InvokeAsync(HttpContext httpContext)
        {
            _log.LogInformation("Invoking");
            try
            {
                if (httpContext.Request.Method.Equals("post", StringComparison.InvariantCultureIgnoreCase) && !httpContext.Request.ContentType.IsNullOrDefault() && httpContext.Request.ContentType.Contains("multipart"))
                {
                    // Get the segments, should be something like /api/campaigns/{id}/contents
                    var segments = httpContext.Request.Path.Value.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                    // If a content post, update it
                    if (segments.Length > 0 && segments[segments.Length - 1].ToLowerInvariant().Equals("content"))
                    {
                        // Verify the creative exists
                        var creativeId = segments[segments.Length - 2];
                        var creative = await _storageCache.Get<Creative>(new StringStorageKey(creativeId), true);
                        if (creative != null)
                        {
                            var header = MediaTypeHeaderValue.Parse(httpContext.Request.ContentType);
                            var boundary = HeaderUtilities.RemoveQuotes(header.Boundary).Value;
                            var reader = new MultipartReader(boundary, httpContext.Request.Body);

                            var section = await reader.ReadNextSectionAsync();
                            while (section != null)
                            {
                                ContentDispositionHeaderValue contentDisposition;
                                if (ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition))
                                {
                                    var fileName = contentDisposition.FileName.Value.Trim('"');
                                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), _config.GetValue("ContentPath", "adcontent"), creative.Id, contentDisposition.FileName.Value.Trim('"'));
                                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                                    _log.LogInformation("Creating {0}", fileName);
                                    _log.LogInformation("ContentType: {0}", section.ContentType);
                                    _log.LogInformation("ContentSize: {0}", section.GetContentDispositionHeader().Size.GetValueOrDefault());

                                    var tempFile = Path.GetTempFileName();
                                    using (var stream = new FileStream(tempFile, FileMode.Create))
                                    {
                                        await section.Body.CopyToAsync(stream);
                                        await stream.FlushAsync();
                                    }

                                    var content = _mediaScanner.Scan(tempFile, section.ContentType);
                                    if (content != null)
                                    {
                                        _log.LogInformation("Adding file {0}", fileName);
                                        File.Copy(tempFile, filePath, true);
                                        File.Delete(tempFile);

                                        content.ContentLocation = filePath;
                                        content.CreativeUri = _config.GetValue("rawUri", "https://east-cdn.lucentbid.com/") + creative.Id + "/" + fileName;
                                        content.RawUri = _config.GetValue("cacheUri", "https://east-cache.lucentbid.com") + "/creatives/" + "/" + creative.Id + "/" + fileName;
                                        var contents = creative.Contents ?? new CreativeContent[0];
                                        Array.Resize(ref contents, contents.Length + 1);
                                        contents[contents.Length - 1] = content;
                                        content.Id = contents.Select(c => c.Id).Max() + 1;
                                        creative.Contents = contents;
                                        _log.LogInformation("Content count {0}", (creative.Contents ?? new CreativeContent[0]).Length);

                                        if (await _storageCache.TryUpdate(creative))
                                        {
                                            httpContext.Response.StatusCode = 201;
                                            return;
                                        }
                                        else
                                        {
                                            File.Delete(fileName);
                                            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        _log.LogWarning("Failed to parse contents");
                                        File.Delete(tempFile);
                                    }
                                }
                                section = await reader.ReadNextSectionAsync();
                            }
                        }
                    }
                    httpContext.Response.StatusCode = 400;
                }
                else
                    await base.InvokeAsync(httpContext);
            }
            catch (Exception e)
            {
                _log.LogError(e, "Creative failure");
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }

        /// <summary>
        /// Purge the contents
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="entity"></param>
        protected override void PostDelete(HttpContext httpContext, Creative entity)
        {
            foreach (var content in entity.Contents ?? new CreativeContent[0])
            {
                File.Delete(content.ContentLocation);
            }

            Directory.Delete(Path.Combine(Directory.GetCurrentDirectory(), _config.GetValue("ContentPath", "adcontent"), entity.Id));
        }
    }
}