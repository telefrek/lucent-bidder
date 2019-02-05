using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Lucent.Common.Bidding;
using Lucent.Common.Entities;
using Lucent.Common.Middleware;
using Lucent.Common.OpenRTB;

namespace Lucent.Common
{
    public static partial class LucentExtensions
    {
        /// <summary>
        /// Replaces the macros in the landing page with the correct information
        /// </summary>
        /// <param name="campaign"></param>
        /// <param name="creative"></param>
        /// <param name="bid"></param>
        /// <returns></returns>
        public static string ReplaceMacros(this Campaign campaign, Creative creative, Bid bid)
        {
            return campaign.LandingPage.Replace(QueryParameters.LUCENT_REDIRECT_PARAMETER, bid.BidContext.GetOperationString(BidOperation.Clicked));
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