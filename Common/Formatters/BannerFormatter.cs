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
        /// <returns></returns>
        public static string ToImageLinkMarkup(this BidContext context)
        {
            if (context.Content.ContentType == ContentType.Banner)
            {
                return @"<a href=""{0}""><img src=""{1}"" width=""{2}"" height=""{3}""></a><img src=""{4}"">".FormatWith(
                    new Uri(context.BaseUri.Uri, "/v1/postback?" + QueryParameters.LUCENT_BID_CONTEXT_PARAMETER + "=" + context.ToString() + "&" + QueryParameters.LUCENT_REDIRECT_PARAMETER + "=" + context.FormatLandingPage().SafeBase64Encode()).AbsoluteUri,
                context.Content.CreativeUri, context.Content.W, context.Content.H, new Uri(context.BaseUri.Uri, "/v1/postback?" + QueryParameters.LUCENT_BID_CONTEXT_PARAMETER + "=" + context.GetOperationString(BidOperation.Impression)).AbsoluteUri);
            }

            return null;
        }
    }
}