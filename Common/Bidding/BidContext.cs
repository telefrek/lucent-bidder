using System;
using System.IO;
using System.Text;
using Lucent.Common.Entities;
using Lucent.Common.Exchanges;

using Lucent.Common.OpenRTB;
using Microsoft.AspNetCore.Http;

namespace Lucent.Common.Bidding
{
    /// <summary>
    /// Represents the context for a given bid
    /// </summary>
    public class BidContext
    {
        /// <summary>
        /// Create a bid context for the request
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static BidContext Create(HttpContext httpContext)
        {
            var context = new BidContext
            {
                Exchange = httpContext.Items["exchange"] as AdExchange,
            };

            context.ExchangeId = context.Exchange.ExchangeId;

            return context;
        }

        /// <summary>
        /// Parse a bid context from a given string
        /// </summary>
        /// <param name="encoded"></param>
        /// <returns></returns>
        public static BidContext Parse(string encoded)
        {
            // Decode the string
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded.SafeBase64Decode()));

            // Read the raw objects
            var context = new BidContext();

            // Read the encoded GUIDs
            context.ExchangeId = decoded.Substring(0, 22).DecodeGuid();
            context.CampaignId = decoded.Substring(22, 22).DecodeGuid();
            context.BidId = decoded.Substring(44, 22).DecodeGuid();

            // Get the other packed values as a protobuf stream
            var packed = Convert.FromBase64String(decoded.Substring(66));
            context.CPM = BitConverter.ToDouble(packed, 0);
            context.BidDate = DateTime.FromFileTimeUtc(BitConverter.ToInt64(packed, 8));
            context.Operation = (BidOperation)BitConverter.ToInt32(packed, 16);
            context.RequestId = Encoding.UTF8.GetString(packed, 20, packed.Length - 20);

            return context;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Guid CampaignId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Guid ExchangeId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Guid BidId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public DateTime BidDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public double CPM { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string RequestId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public BidOperation Operation { get; set; }

        /// <summary>
        /// The request the bid is scoped to
        /// </summary>
        /// <value></value>
        public BidRequest Request { get; set; }

        /// <summary>
        /// The impression the bid is for
        /// </summary>
        /// <value></value>
        public Impression Impression { get; set; }

        /// <summary>
        /// Get the base uri for the callbacks
        /// </summary>
        /// <value></value>
        public UriBuilder BaseUri { get; set; }

        /// <summary>
        /// Get the exchange in use
        /// </summary>
        /// <value></value>
        public AdExchange Exchange { get; set; }

        /// <summary>
        /// Get the current campaign in use
        /// </summary>
        /// <value></value>
        public Campaign Campaign { get; set; }

        /// <summary>
        /// The current creative in use
        /// </summary>
        /// <value></value>
        public Creative Creative { get; set; }

        /// <summary>
        /// Get the 
        /// </summary>
        /// <value></value>
        public CreativeContent Content { get; set; }

        /// <summary>
        /// The bid
        /// </summary>
        /// <value></value>
        public Bid Bid { get; set; }

        /// <summary>
        /// Get the operation parameter
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        public string GetOperationString(BidOperation operation)
        {
            this.Operation = operation;
            return this.ToString();
        }

        /// <summary>
        /// Override the default ToString method to get encoded
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            // Encode the guids
            sb.Append(ExchangeId.EncodeGuid());
            sb.Append(CampaignId.EncodeGuid());
            sb.Append(BidId.EncodeGuid());

            // Wre the additional values using protobuf
            using (var ms = new MemoryStream())
            {
                ms.Write(BitConverter.GetBytes(CPM));
                ms.Write(BitConverter.GetBytes(BidDate.ToFileTimeUtc()));
                ms.Write(BitConverter.GetBytes((int)Operation));
                ms.Write(Encoding.UTF8.GetBytes(RequestId));
                ms.Flush();

                sb.Append(Convert.ToBase64String(ms.ToArray()));
            }

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString())).SafeBase64Encode();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum BidOperation
    {
        /// <value></value>
        Unknown = 0,
        /// <value></value>
        Win,
        /// <value></value>
        Loss,
        /// <value></value>
        Impression,
        /// <value></value>
        Clicked,
        /// <value></value>
        Action,
        /// <value></value>
        Viewed,
        /// <value></value>
        NotViewed,
        /// <value></value>
        Error,
    }
}