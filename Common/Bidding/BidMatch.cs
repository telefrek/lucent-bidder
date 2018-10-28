using Lucent.Common.Entities;
using Lucent.Common.OpenRTB;

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
    }
}