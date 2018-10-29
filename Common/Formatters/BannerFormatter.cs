using System;
using Lucent.Common.Bidding;
using Lucent.Common.Entities;
using Lucent.Common.Middleware;

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
        /// <param name="context"></param>
        /// <param name="baseUri"></param>
        /// <returns></returns>
        public static string ToImageLinkMarkup(this BidMatch match, BidContext context, Uri baseUri)
        {

            if (match.Content.ContentType == ContentType.Banner)
            {
                return @"<a href=""{0}""><img src=""{1}"" width=""{2}"" height=""{3}""></a>".FormatWith(
                    new Uri(baseUri, "/v1/postback?" + QueryParameters.LUCENT_BID_CONTEXT_PARAMETER + "=" + context.GetOperationString(BidOperation.Clicked) + "&" + QueryParameters.LUCENT_REDIRECT_PARAMETER + "=" + match.Campaign.LandingPage.SafeBase64Encode()).AbsoluteUri,
                match.Content.CreativeUri, match.Content.H, match.Content.W);
            }

            return null;
        }
    }
}