using Lucent.Common.Bidding;
using Lucent.Common.Entities;
using Lucent.Common.OpenRTB;

namespace Lucent.Common.Formatters
{
    /// <summary>
    /// Base class to generate markup
    /// </summary>
    public class MarkupGenerator
    {
        /// <summary>
        /// Generates markup for the bid
        /// </summary>
        /// <param name="bidContext">The context used for the bid</param>
        /// <returns></returns>
        public virtual void GenerateMarkup(BidContext bidContext)
        {
            switch (bidContext.Content.ContentType)
            {
                case ContentType.Banner:
                    bidContext.Bid.AdMarkup = bidContext.ToImageLinkMarkup();
                    break;
                case ContentType.Video:
                    bidContext.Bid.AdMarkup = bidContext.ToVast();
                    break;
            }
        }
    }
}