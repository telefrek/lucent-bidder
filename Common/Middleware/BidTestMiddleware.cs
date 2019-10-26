using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucent.Common.Bidding;
using Lucent.Common.Caching;
using Lucent.Common.Entities;
using Lucent.Common.Exchanges;
using Lucent.Common.Formatters;
using Lucent.Common.Messaging;
using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;
using Lucent.Common.Storage;
using Lucent.Common.UserManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace Lucent.Common.Middleware
{
    /// <summary>
    /// Middleware for controlling bids
    /// </summary>
    public class BidTestMiddleware
    {
        ILogger<BidTestMiddleware> _log;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        public BidTestMiddleware(RequestDelegate next, ILogger<BidTestMiddleware> logger)
        {
            _log = logger;
        }

        /// <summary>
        /// Handle the request asynchronously
        /// </summary>
        /// <param name="httpContext">The current http context</param>
        /// <param name="storageManager">The storage manager instance</param>
        /// <returns>A completed pipeline step</returns>
        public async Task InvokeAsync(HttpContext httpContext, IStorageManager storageManager)
        {
            var campaignId = httpContext.Request.Query
                .FirstOrDefault(kvp => kvp.Key.Equals("campaign", StringComparison.InvariantCultureIgnoreCase)).Value.ToString();
            var creativeId = httpContext.Request.Query
                .FirstOrDefault(kvp => kvp.Key.Equals("creative", StringComparison.InvariantCultureIgnoreCase)).Value.ToString();
            var contentId = int.Parse(httpContext.Request.Query
                .FirstOrDefault(kvp => kvp.Key.Equals("content", StringComparison.InvariantCultureIgnoreCase)).Value);

            _log.LogInformation("Starting bid test");

            try
            {
                var campaign = await storageManager.GetRepository<Campaign>().Get(campaignId);

                if (campaign.CreativeIds.Any(c => c.Equals(creativeId.ToString(), StringComparison.InvariantCulture)))
                {
                    var creative = await storageManager.GetRepository<Creative>().Get(creativeId);

                    if (creative.Contents.Length > contentId)
                    {
                        var content = creative.Contents[contentId];

                        _log.LogInformation("Found campaign, creative and content");

                        // Need some uri building
                        var baseUri = new UriBuilder
                        {
                            Scheme = "https",
                            Host = httpContext.Request.Host.Value,
                        };

                        var bidContext = new BidContext { ExchangeId = SequentialGuid.NextGuid() };
                        bidContext.Request = new BidRequest { Id = SequentialGuid.NextGuid().ToString() };
                        bidContext.Impression = new Impression { Banner = new Banner { H = content.H, W = content.W, MimeTypes = new string[] { content.MimeType } } };
                        bidContext.Campaign = campaign;
                        bidContext.Creative = creative;
                        bidContext.Content = content;
                        bidContext.BidId = SequentialGuid.NextGuid();
                        bidContext.ExchangeId = SequentialGuid.NextGuid();
                        bidContext.RequestId = bidContext.Request.Id;
                        bidContext.CampaignId = Guid.Parse(campaign.Id);

                        bidContext.BaseUri = baseUri;
                        bidContext.BidDate = DateTime.UtcNow;
                        bidContext.Bid = new Bid
                        {
                            ImpressionId = bidContext.Impression.ImpressionId,
                            Id = bidContext.BidId.ToString(),
                            CPM = 1,
                            WinUrl = new Uri(baseUri.Uri, "/v1/postback?" + QueryParameters.LUCENT_BID_CONTEXT_PARAMETER + "=" + bidContext.GetOperationString(BidOperation.Win)).AbsoluteUri + "&cpm=${AUCTION_PRICE}",
                            LossUrl = new Uri(baseUri.Uri, "/v1/postback?" + QueryParameters.LUCENT_BID_CONTEXT_PARAMETER + "=" + bidContext.GetOperationString(BidOperation.Loss)).AbsoluteUri,
                            BillingUrl = new Uri(baseUri.Uri, "/v1/postback?" + QueryParameters.LUCENT_BID_CONTEXT_PARAMETER + "=" + bidContext.GetOperationString(BidOperation.Impression)).AbsoluteUri,
                            H = bidContext.Content.H,
                            W = bidContext.Content.W,
                            AdDomain = bidContext.Campaign.AdDomains.ToArray(),
                            BidExpiresSeconds = 300,
                            Bundle = bidContext.Campaign.BundleId,
                            ContentCategories = bidContext.Content.Categories,
                            ImageUrl = bidContext.Content.RawUri,
                            AdId = bidContext.Creative.Id,
                            CreativeId = bidContext.Creative.Id + "." + bidContext.Content.Id,
                            CampaignId = bidContext.Campaign.Id,
                        };
                        bidContext.CPM = 1;

                        _log.LogInformation(bidContext.FormatLandingPage());

                        new MarkupGenerator().GenerateMarkup(bidContext);

                        httpContext.Response.StatusCode = 200;
                        httpContext.Response.ContentType = "text/html";
                        await httpContext.Response.WriteAsync(bidContext.Bid.AdMarkup, Encoding.UTF8);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to generate a bid");
                httpContext.Response.StatusCode = 501;
            }

            httpContext.Response.StatusCode = 400;
        }
    }
}