using System;
using Lucent.Common.Entities;
using Lucent.Common.Exchanges;
using Lucent.Common.OpenRTB;
using Microsoft.AspNetCore.Http;

namespace Lucent.Common.Bidding
{
    /// <summary>
    /// 
    /// </summary>
    public class BidMatch
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public BidRequest Request { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Impression Impression { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public CreativeContent Content { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Campaign Campaign { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Creative Creative { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Bid RawBid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public BidContext CreateContext(HttpContext httpContext)
        {
            var bidContext = new BidContext
            {
                BidDate = DateTime.UtcNow,
                CampaignId = Guid.Parse(Campaign.Id),
                ExchangeId = (httpContext.Items["exchange"] as AdExchange).ExchangeId,
                RequestId = Request.Id,
            };

            if (RawBid != null)
            {
                bidContext.BidId = Guid.Parse(RawBid.Id);
                bidContext.CPM = RawBid.CPM;
            }
            else
            {
                bidContext.BidId = SequentialGuid.NextGuid();
            }

            return bidContext;
        }
    }
}