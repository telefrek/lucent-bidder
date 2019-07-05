using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Lucent.Common.Bidding;
using Lucent.Common.Entities;
using Lucent.Common.Middleware;
using Lucent.Common.OpenRTB;

namespace Lucent.Common
{
    public static partial class LucentExtensions
    {
        static readonly Regex _bidTokenizer = new Regex(@"{([^{}]*)}", RegexOptions.Compiled);

        /// <summary>
        /// Replaces the macros in the landing page with the correct information
        /// </summary>
        /// <param name="bidContext"></param>
        /// <returns></returns>
        public static string FormatLandingPage(this BidContext bidContext)
        {
            var tokens = _bidTokenizer.Matches(bidContext.Campaign.LandingPage);
            var s = bidContext.Campaign.LandingPage;

            var device = bidContext.Request.Device ?? new Device { Geo = new Geo() };

            foreach (var token in tokens)
            {
                switch (token.ToString().ToLower())
                {
                    case "{adv_id}":
                        s = s.Replace(token.ToString(), "");
                        break;
                    case "{click_id}":
                        s = s.Replace(token.ToString(), "");
                        break;
                    case "{pub_id}":
                        s = s.Replace(token.ToString(), bidContext.Request.App != null ? bidContext.Request.App.Name ?? bidContext.Request.App.Id ?? "" : bidContext.Request.Site != null ? bidContext.Request.Site.Name ?? bidContext.Request.Site.Id ?? "" : "");
                        break;
                    case "{exc_id}":
                        s = s.Replace(token.ToString(), bidContext.ExchangeId.EncodeGuid());
                        break;
                    case "{city}":
                        s = s.Replace(token.ToString(), device.Geo.City ?? "");
                        break;
                    case "{country_code}":
                        s = s.Replace(token.ToString(), "");
                        break;
                    case "{currency}":
                        s = s.Replace(token.ToString(), "USD");
                        break;
                    case "{date}":
                        s = s.Replace(token.ToString(), DateTime.UtcNow.ToString("yyyy-MM-dd"));
                        break;
                    case "{datetime}":
                        s = s.Replace(token.ToString(), DateTime.UtcNow.ToString("o"));
                        break;
                    case "{time}":
                        s = s.Replace(token.ToString(), DateTime.UtcNow.ToString("hh:mm:ss"));
                        break;
                    case "{ip}":
                        s = s.Replace(token.ToString(), device.Ipv4 ?? "");
                        break;
                    case "{carrier}":
                        s = s.Replace(token.ToString(), device.Carrier ?? "");
                        break;
                    case "{payout}":
                        s = s.Replace(token.ToString(), "");
                        break;
                    case "{purchase}":
                        s = s.Replace(token.ToString(), "");
                        break;
                    case "{conversion_id}":
                        s = s.Replace(token.ToString(), "");
                        break;
                    case "{event}":
                        s = s.Replace(token.ToString(), "");
                        break;
                    case "{device_brand}":
                        // TODO: Figure out how to do this
                        s = s.Replace(token.ToString(), "");
                        break;
                    case "{device_model}":
                        s = s.Replace(token.ToString(), device.Model ?? "");
                        break;
                    case "{device_os}":
                        s = s.Replace(token.ToString(), device.OS ?? "");
                        break;
                    case "{device_os_version}":
                        s = s.Replace(token.ToString(), device.OSVersion ?? "");
                        break;
                    case "{google_aid}":
                        s = s.Replace(token.ToString(), "");
                        break;
                    case "{android_id}":
                        s = s.Replace(token.ToString(), "");
                        break;
                    case "{idfa}":
                        s = s.Replace(token.ToString(), device.Id ?? "");
                        break;
                    case "{unid}":
                        s = s.Replace(token.ToString(), "");
                        break;
                    case "{user_id}":
                        s = s.Replace(token.ToString(), "");
                        break;
                    case "{creative_id}":
                        s = s.Replace(token.ToString(), bidContext.Content.Id.ToString());
                        break;
                    case "{creative_group}":
                        s = s.Replace(token.ToString(), bidContext.Creative.Id);
                        break;
                    case "{" + QueryParameters.LUCENT_BID_CONTEXT_PARAMETER + "}":
                        s = s.Replace(token.ToString(), bidContext.GetOperationString(BidOperation.Action));
                        break;
                    default:
                        s = s.Replace(token.ToString(), "");
                        break;
                }
            }

            return s;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        public static void HydrateFilter(this CreativeContent content)
        {
            var impParam = Expression.Parameter(typeof(Impression), "imp");
            var expressions = new List<Expression>();

            var ret = Expression.Label(typeof(bool)); // We're going to return a bool

            if (content.ContentType == ContentType.Banner)
            {
                // Banner Filters
                var bValue = Expression.Property(impParam, "Banner");

                var bannerExpressions = new List<Expression>();

                var hProp = Expression.Property(bValue, "H");
                var wProp = Expression.Property(bValue, "W");

                var hMin = Expression.Property(bValue, "HMin");
                var hMax = Expression.Property(bValue, "HMax");

                var wMin = Expression.Property(bValue, "WMin");
                var wMax = Expression.Property(bValue, "WMax");

                // If H < hMin || H > hMax
                bannerExpressions.Add(Expression.IfThen(Expression.OrElse(Expression.AndAlso(Expression.GreaterThan(hMax, Expression.Constant(0)), Expression.LessThan(hMax, Expression.Constant(content.H))), Expression.GreaterThan(hMin, Expression.Constant(content.H))), Expression.Return(ret, Expression.Constant(true))));

                // If W < wMin || W > wMax
                bannerExpressions.Add(Expression.IfThen(Expression.OrElse(Expression.AndAlso(Expression.GreaterThan(wMax, Expression.Constant(0)), Expression.LessThan(wMax, Expression.Constant(content.W))), Expression.GreaterThan(wMin, Expression.Constant(content.W))), Expression.Return(ret, Expression.Constant(true))));

                if (content.CanScale)
                {
                    // Test of impression h / w == content h / w
                    bannerExpressions.Add(Expression.IfThen(
                        Expression.Equal(
                            Expression.Divide(
                                Expression.Multiply(
                                    hProp, Expression.Constant(1.0d)),
                                Expression.Multiply(
                                    wProp, Expression.Constant(1.0d))
                            ),
                            Expression.Constant((content.H * 1.0d) / (content.W * 1.0d))
                        ),
                        Expression.Return(ret, Expression.Constant(true))));
                }
                else
                {
                    // Test if content.h/w == imp.h/w
                    bannerExpressions.Add(Expression.IfThen(
                        Expression.OrElse(
                            Expression.NotEqual(
                                hProp, Expression.Constant(content.H)),
                            Expression.NotEqual(
                                wProp, Expression.Constant(content.W))
                        ), Expression.Return(ret, Expression.Constant(true))));
                }

                // Create a block
                var bannerBlock = Expression.Block(bannerExpressions);

                // Execute the block if the banner property is not null
                expressions.Add(Expression.IfThen(Expression.NotEqual(bValue, Expression.Constant(null)), bannerBlock));

                // Add the default return false for the end
                expressions.Add(Expression.Return(ret, Expression.Constant(false)));
            }
            else if (content.ContentType == ContentType.Video)
            {
                // Video Filters
                var vValue = Expression.Property(impParam, "Video");

                expressions.Add(Expression.Return(ret, Expression.Constant(true)));
            }
            else
            {
                expressions.Add(Expression.Return(ret, Expression.Constant(true)));
            }

            expressions.Add(Expression.Label(ret, Expression.Constant(false)));

            // Compile the expression block
            var final = Expression.Block(expressions);

            // Build the method
            var ftype = typeof(Func<,>).MakeGenericType(typeof(Impression), typeof(bool));
            var comp = makeLambda.MakeGenericMethod(ftype).Invoke(null, new object[] { final, new ParameterExpression[] { impParam } });

            // Compile the filter
            content.Filter = (Func<Impression, bool>)comp.GetType().GetMethod("Compile", Type.EmptyTypes).Invoke(comp, new object[] { });

        }
    }
}