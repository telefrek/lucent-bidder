using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Entities;
using Lucent.Common.Filters;
using Lucent.Common.Filters.Serializers;
using Lucent.Common.OpenRTB;
using Lucent.Common.Serialization;

namespace Lucent.Common
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class LucentExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="loopVar"></param>
        /// <param name="loopContent"></param>
        /// <returns></returns>
        public static Expression ForEach(Expression collection, ParameterExpression loopVar, Expression loopContent)
        {
            var elementType = loopVar.Type;
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(elementType);

            var enumeratorVar = Expression.Variable(enumeratorType, "e");
            var getEnumeratorCall = Expression.Call(collection, enumerableType.GetMethod("GetEnumerator"));
            var enumeratorAssign = Expression.Assign(enumeratorVar, getEnumeratorCall);

            var moveNextCall = Expression.Call(enumeratorVar, typeof(IEnumerator).GetMethod("MoveNext"));

            var breakLabel = Expression.Label("lbrk");

            var loop = Expression.Block(new[] { enumeratorVar },
                enumeratorAssign,
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Equal(moveNextCall, Expression.Constant(true)),
                        Expression.Block(new[] { loopVar },
                            Expression.Assign(loopVar, Expression.Property(enumeratorVar, "Current")),
                            loopContent
                        ),
                        Expression.Break(breakLabel)
                    ),
                breakLabel)
            );

            return loop;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bidFilter"></param>
        /// <returns></returns>
        public static Func<BidRequest, bool> GenerateCode(this BidFilter bidFilter)
        {
            // Need our input parameter :)
            var bidParam = Expression.Parameter(typeof(BidRequest), "bid");

            // Need a variable to track the complex filtering
            var fValue = Expression.Variable(typeof(bool), "isFiltered");

            // Keep track of all the expressions in this chain
            var expList = new List<Expression> { };

            // Need a sentinal value for breaking in loops
            var loopBreak = Expression.Label();
            var ret = Expression.Label(typeof(bool)); // We're going to return a bool

            // Process the impressions filters
            if (bidFilter.ImpressionFilters != null)
            {
                // Get the impressions
                var impProp = Expression.Property(bidParam, "Impressions");
                var impType = typeof(Impression);

                // Create a loop parameter and the filter
                var impParam = Expression.Parameter(impType, "imp");
                var impTest = CombineFilters(bidFilter.ImpressionFilters, impParam);

                // Create the loop and add it to this block
                expList.Add(Expression.IfThen(Expression.NotEqual(impProp, Expression.Constant(null)),
                    ForEach(impProp, impParam, Expression.IfThen(impTest, Expression.Return(ret, Expression.Constant(true))))));
            }

            // Process the user filters
            if (bidFilter.UserFilters != null)
            {
                // Get the user property and filters
                var userProp = Expression.Property(bidParam, "User");
                var userType = typeof(User);
                var userFilter = CombineFilters(bidFilter.UserFilters, userProp);

                // Get the geographical filter and property
                var geoProp = Expression.Property(userProp, "Geo");
                var geoFilter = (Expression)Expression.Constant(false);
                if (bidFilter.GeoFilters != null)
                    geoFilter = CombineFilters(bidFilter.GeoFilters, geoProp);

                // Add the filter if(usr != null
                expList.Add(Expression.IfThen(
                    Expression.AndAlso(Expression.NotEqual(userProp, Expression.Constant(null)), Expression.OrElse(userFilter, Expression.AndAlso(Expression.NotEqual(geoProp, Expression.Constant(null)), geoFilter))), Expression.Return(ret, Expression.Constant(true))));
            }

            expList.Add(Expression.Label(ret, Expression.Constant(false)));

            var final = Expression.Block(expList);

            var ftype = typeof(Func<,>).MakeGenericType(typeof(BidRequest), typeof(bool));
            var comp = makeLambda.MakeGenericMethod(ftype).Invoke(null, new object[] { final, new ParameterExpression[] { bidParam } });
            return (Func<BidRequest, bool>)comp.GetType().GetMethod("Compile", Type.EmptyTypes).Invoke(comp, new object[] { });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Expression CreateExpression(this Filter filter, Expression p)
        {
            var prop = Expression.Property(p, filter.Property);

            Expression exp = null;
            switch (filter.FilterType)
            {
                case FilterType.NEQ:
                    exp = Expression.NotEqual(prop, Expression.Constant(filter.Value));
                    break;
                case FilterType.GT:
                    exp = Expression.GreaterThan(prop, Expression.Constant(filter.Value));
                    break;
                case FilterType.GTE:
                    exp = Expression.GreaterThanOrEqual(prop, Expression.Constant(filter.Value));
                    break;
                case FilterType.LT:
                    exp = Expression.LessThan(prop, Expression.Constant(filter.Value));
                    break;
                case FilterType.LTE:
                    exp = Expression.LessThanOrEqual(prop, Expression.Constant(filter.Value));
                    break;
                case FilterType.IN:
                case FilterType.NOTIN:
                    // Need to check types to do this efficiently ?
                    // if prop is string, contains
                    // else if prop is array, for blah in Length...
                    // else if prop is collection, Linq.Contains
                    if (filter.Values != null)
                    {

                    }
                    else
                    {

                    }

                    // Not in is just assert in != true
                    if (filter.FilterType == FilterType.NOTIN)
                        exp = Expression.NotEqual(exp, Expression.Constant(true));
                    break;
                default:
                    exp = Expression.Equal(prop, Expression.Constant(filter.Value));
                    break;
            }

            return exp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Expression CombineFilters(this ICollection<Filter> filters, Expression target)
        {
            Expression exp = null;

            foreach (var filter in filters)
            {
                var e = filter.CreateExpression(target);

                if (exp == null)
                    exp = e;
                else
                    exp = Expression.OrElse(exp, e);
            }

            return exp;
        }

        static readonly MethodInfo makeLambda = typeof(Expression).GetMethods().Where(m =>
                m.Name == "Lambda" && m.IsGenericMethod && m.GetGenericArguments().Length == 1
                ).First();
    }
}