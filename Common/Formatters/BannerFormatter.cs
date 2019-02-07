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
        /// <param name="context"></param>
        /// <param name="baseUri"></param>
        /// <returns></returns>
        public static string ToImageLinkMarkup(this BidContext context, Uri baseUri)
        {
            if (context.Content.ContentType == ContentType.Banner)
            {
                return @"<a href=""{0}""><img src=""{1}"" width=""{2}"" height=""{3}""></a>".FormatWith(
                    new Uri(baseUri, "/v1/postback?" + QueryParameters.LUCENT_BID_CONTEXT_PARAMETER + "=" + context.ToString() + "&" + QueryParameters.LUCENT_REDIRECT_PARAMETER + "=" + context.Campaign.LandingPage.SafeBase64Encode()).AbsoluteUri,
                context.Content.CreativeUri, context.Content.H, context.Content.W);
            }

            return null;
        }
    }
}