using Lucent.Common.Bidding;
using Lucent.Common.Entities;

namespace Lucent.Common.Formatters
{
    /// <summary>
    /// Format banner text
    /// </summary>
    public static class BannerFormatter
    {
        /// <summary>
        /// Format the matched bid markup
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public static string ToImageLinkMarkup(this BidMatch match)
        {
            if (match.Content.ContentType == ContentType.Banner)
            {
                return @"<a href=""{0}""><img src=""{1}"" width=""{2}"" height=""{3}""></a>".FormatWith(
                    match.Campaign.LandingPage,
                match.Content.CreativeUri, match.Content.H, match.Content.W);
            }

            return null;
        }
    }
}