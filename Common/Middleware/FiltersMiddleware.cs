using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Lucent.Common.Caching;
using Lucent.Common.Entities;
using Lucent.Common.Filters;
using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lucent.Common.Middleware
{
    /// <summary>
    /// Handles JSON -> Filter
    /// </summary>
    public class FiltersMiddleware
    {
        private readonly ISerializationContext _serializationContext;
        private readonly StorageCache _storageCache;
        private readonly ILogger _logger;

        /// <summary>
        /// Injection constructor
        /// </summary>
        /// <param name="next">The next handler in the chain</param>
        /// <param name="serializationContext">Current context</param>
        /// <param name="storageCache">The storage cache to use</param>
        /// <param name="logger">A logger</param>
        public FiltersMiddleware(RequestDelegate next, ISerializationContext serializationContext, StorageCache storageCache, ILogger<FiltersMiddleware> logger)
        {
            _serializationContext = serializationContext;
            _storageCache = storageCache;
            _logger = logger;
        }

        /// <summary>
        /// Invocation handler
        /// </summary>
        /// <param name="httpContext">The current request context</param>
        /// <returns>A task</returns>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                var segments = httpContext.Request.Path.Value.Split("/", StringSplitOptions.RemoveEmptyEntries);

                var body = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
                var jsonArr = new JsonSerializer().Deserialize<JsonFilter[]>(new JsonTextReader(new StringReader(body)));

                _logger.LogInformation("Body : {0}", body);

                var campaign = await _storageCache.Get<Campaign>(new StringStorageKey(segments.Last()), true);

                foreach (var jsonObj in jsonArr)
                {
                    var filter = new Filter
                    {
                        FilterType = Enum.Parse<FilterType>(jsonObj.operation, true),
                        Value = jsonObj.values.Length == 1 ? FilterValue.Cast(jsonObj.values.Last(), _logger) : null,
                        Values = jsonObj.values.Length > 1 ? jsonObj.values.Select(v => FilterValue.Cast(v, _logger)).ToArray() : null,
                    };

                    if (campaign.BidFilter == null)
                        campaign.BidFilter = new BidFilter();

                    switch (jsonObj.entity)
                    {
                        case "geo":
                            if (TryParseProperty<Geo>(filter, jsonObj))
                            {
                                var filters = campaign.BidFilter.GeoFilters ?? new Filter[0];
                                Array.Resize(ref filters, filters.Length + 1);
                                filters[filters.Length - 1] = filter;
                                campaign.BidFilter.GeoFilters = filters;
                            }
                            break;
                        case "device":
                            if (TryParseProperty<Device>(filter, jsonObj))
                            {
                                var filters = campaign.BidFilter.DeviceFilters ?? new Filter[0];
                                Array.Resize(ref filters, filters.Length + 1);
                                filters[filters.Length - 1] = filter;
                                campaign.BidFilter.DeviceFilters = filters;
                            }
                            break;
                        case "app":
                            if (TryParseProperty<App>(filter, jsonObj))
                            {
                                var filters = campaign.BidFilter.AppFilters ?? new Filter[0];
                                Array.Resize(ref filters, filters.Length + 1);
                                filters[filters.Length - 1] = filter;
                                campaign.BidFilter.AppFilters = filters;
                            }
                            break;
                        case "site":
                            if (TryParseProperty<Site>(filter, jsonObj))
                            {
                                var filters = campaign.BidFilter.SiteFilters ?? new Filter[0];
                                Array.Resize(ref filters, filters.Length + 1);
                                filters[filters.Length - 1] = filter;
                                campaign.BidFilter.SiteFilters = filters;
                            }
                            break;
                        case "impression":
                            if (TryParseProperty<Impression>(filter, jsonObj))
                            {
                                var filters = campaign.BidFilter.ImpressionFilters ?? new Filter[0];
                                Array.Resize(ref filters, filters.Length + 1);
                                filters[filters.Length - 1] = filter;
                                campaign.BidFilter.ImpressionFilters = filters;
                            }
                            break;
                        case "user":
                            if (TryParseProperty<User>(filter, jsonObj))
                            {
                                var filters = campaign.BidFilter.UserFilters ?? new Filter[0];
                                Array.Resize(ref filters, filters.Length + 1);
                                filters[filters.Length - 1] = filter;
                                campaign.BidFilter.UserFilters = filters;
                            }
                            break;
                    }
                }

                if (await _storageCache.TryUpdate(campaign))
                {
                    httpContext.Response.StatusCode = 202;
                    return;
                }

                httpContext.Response.StatusCode = 404;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to process request");
                httpContext.Response.StatusCode = 400;
            }
        }

        bool TryParseProperty<T>(Filter filter, JsonFilter jsonObj)
        {
            var property = jsonObj.property;
            switch (property.ToLower())
            {
                case "ispaid":
                    property = "ispaidversion";
                    break;
                case "appcategory":
                    property = "appcategories";
                    break;
                case "sectioncategory":
                    property = "sectioncategories";
                    break;
                case "pagecategory":
                    property = "pagecategories";
                    break;
                case "sitecategory":
                    property = "sitecategories";
                    break;
                case "issecure":
                    property = "IsHttpsRequired";
                    break;
                case "os_version":
                    property = "osversion";
                    break;
                case "type":
                    property = "geotype";
                    break;
            }
            _logger.LogInformation("Parsing {0} ({1})", property, jsonObj.property);

            var prop = typeof(T).GetProperty(property, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
            if (prop != null)
            {
                filter.Property = prop.Name;
                filter.PropertyType = prop.PropertyType;
                return true;
            }

            _logger.LogWarning("failed");

            return false;
        }
    }

    class JsonFilter
    {
        public string operation { get; set; }
        public string entity { get; set; }
        public string property { get; set; }
        public object[] values { get; set; }
    }
}