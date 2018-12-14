using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Lucent.Common.Entities;
using Lucent.Common.Filters;
using Lucent.Common.OpenRTB;

// Enumerable

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

            // Process the site filters
            if (bidFilter.SiteFilters != null)
            {
                // Get the impressions
                var siteProp = Expression.Property(bidParam, "Site");
                var siteType = typeof(Site);
                var siteTest = CombineFilters(bidFilter.SiteFilters, siteProp);

                // Create the loop and add it to this block
                expList.Add(Expression.IfThen(Expression.AndAlso(Expression.NotEqual(siteProp, Expression.Constant(null)),
                    siteTest), Expression.Return(ret, Expression.Constant(true))));
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
            var ptype = (prop.Member as PropertyInfo).PropertyType;

            Expression exp = Expression.Constant(true);
            var fExpVal = Expression.Constant(typeof(LucentExtensions).GetMethods()
                                            .Single(m => m.IsGenericMethod && m.Name == "CastTo").MakeGenericMethod(ptype.IsArray ? ptype.GetElementType() : ptype).Invoke(null, new object[] { filter.Value }));

            var fValsExp = Expression.Constant(typeof(LucentExtensions).GetMethods()
                                            .Single(m => m.IsGenericMethod && m.Name == "CastArrayTo").MakeGenericMethod(ptype.IsArray ? ptype.GetElementType() : ptype).Invoke(null, new object[] { filter.Values }));
            switch (filter.FilterType)
            {
                case FilterType.NEQ:
                    exp = Expression.NotEqual(prop, fExpVal);
                    break;
                case FilterType.GT:
                    exp = Expression.GreaterThan(prop, fExpVal);
                    break;
                case FilterType.GTE:
                    exp = Expression.GreaterThanOrEqual(prop, fExpVal);
                    break;
                case FilterType.LT:
                    exp = Expression.LessThan(prop, fExpVal);
                    break;
                case FilterType.LTE:
                    exp = Expression.LessThanOrEqual(prop, fExpVal);
                    break;
                case FilterType.IN:
                case FilterType.NOTIN:
                    if (ptype.IsArray)
                    {
                        if (filter.Values != null)
                        {
                            // Hahahaha...this is ugly
                            exp = Expression.Call(null,
                                typeof(Enumerable).GetMethods().Single(m => m.Name.Equals("Intersect") && m.IsGenericMethod && m.GetParameters().Length == 2)
                                    .MakeGenericMethod(ptype.GetElementType()),
                                        prop,
                                        fValsExp);

                            var m1 = typeof(Enumerable).GetMethods().Single(m => m.Name.Equals("Count") && m.IsGenericMethod && m.GetParameters().Length == 1).MakeGenericMethod(ptype.GetElementType());
                            exp = Expression.GreaterThan(Expression.Call(null, m1, exp), Expression.Constant(0));
                        }
                        else if (filter.Value != null)
                        {
                            exp = Expression.Call(null,
                                typeof(Enumerable).GetMethods().Single(m => m.Name.Equals("Contains") && m.IsGenericMethod && m.GetParameters().Length == 2)
                                    .MakeGenericMethod(ptype.GetElementType()), prop,
                                        Expression.Convert(fExpVal, ptype.GetElementType()));
                        }
                    }
                    else if (ptype.IsAssignableFrom(typeof(string)))
                    {
                        if (filter.Values != null)
                        {
                            var lamRet = Expression.Label(typeof(bool));
                            var lamParam = Expression.Parameter(typeof(string), "s");
                            var expList = new List<Expression>();
                            expList.Add(Expression.IfThen(Expression.Call(prop, typeof(string).GetMethods().First(m => m.Name == "Contains" && m.GetParameters().Length == 1 && m.GetParameters().First().ParameterType == typeof(string)), lamParam), Expression.Return(lamRet, Expression.Constant(true))));

                            expList.Add(Expression.Label(lamRet, Expression.Constant(false)));

                            var final = Expression.Block(expList);
                            var ftype = typeof(Func<,>).MakeGenericType(typeof(string), typeof(bool));

                            exp = Expression.Call(null, typeof(Enumerable).GetMethods().Single(m => m.Name.Equals("Any") && m.IsGenericMethod && m.GetParameters().Length == 2).MakeGenericMethod(typeof(string)), fValsExp, Expression.Lambda(final, false, new ParameterExpression[] { lamParam }));

                        }
                        else if (filter.Value != null)
                        {
                            exp = Expression.Call(prop, typeof(string).GetMethods().First(m => m.Name == "Contains" && m.GetParameters().Length == 1 && m.GetParameters().First().ParameterType == typeof(string)), fExpVal);
                        }
                    }
                    else if (ptype.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)) != null)
                    {
                        // This never gets used right now, depends on definition
                        if (filter.Values != null)
                        {
                            // Hahahaha...this is ugly
                            exp = Expression.Call(null,
                                typeof(Enumerable).GetMethods().Single(m => m.Name.Equals("Intersect") && m.IsGenericMethod && m.GetParameters().Length == 2)
                                    .MakeGenericMethod(ptype.GetGenericArguments()[0]),
                                        prop,
                                        fValsExp);

                            var m1 = typeof(Enumerable).GetMethods().Single(m => m.Name.Equals("Count") && m.IsGenericMethod && m.GetParameters().Length == 1).MakeGenericMethod(ptype.GetGenericArguments()[0]);
                            exp = Expression.GreaterThan(Expression.Call(null, m1, exp), Expression.Constant(0));
                        }
                        else if (filter.Value != null)
                        {
                            exp = Expression.Call(null,
                                typeof(Enumerable).GetMethods().Single(m => m.Name.Equals("Contains") && m.IsGenericMethod && m.GetParameters().Length == 2)
                                    .MakeGenericMethod(ptype.GetElementType()), prop,
                                        Expression.Convert(fExpVal, ptype.GetElementType()));
                        }
                    }
                    else
                        return exp; // Don't evaluate the not in

                    // Not in is just assert in != true
                    if (filter.FilterType == FilterType.NOTIN)
                        exp = Expression.NotEqual(exp, Expression.Constant(true));
                    break;
                default:
                    exp = Expression.Equal(prop, fExpVal);
                    break;
            }

            return Expression.AndAlso(Expression.NotEqual(prop, Expression.Constant(ptype.IsValueType ? Activator.CreateInstance(ptype) : null)), exp);
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

        /// <summary>
        /// Cast the object array to the correct type
        /// </summary>
        /// <param name="original"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] CastArrayTo<T>(FilterValue[] original)
        {
            if (original == null)
                return new T[0];

            if (typeof(T) == typeof(string))
                return original.Select(o => (T)(object)o.SValue).ToArray();
            else if (typeof(T) == typeof(int))
                return original.Select(o => (T)(object)o.IValue).ToArray();

            return new T[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CastTo<T>(FilterValue filter)
        {
            if (filter == null)
                return default(T);

            if (typeof(T) == typeof(string))
                return (T)(object)(string)filter;
            else if (typeof(T) == typeof(int))
                return (T)(object)(int)filter;

            return default(T);
        }

        static readonly MethodInfo makeLambda = typeof(Expression).GetMethods().Where(m =>
                m.Name == "Lambda" && m.IsGenericMethod && m.GetGenericArguments().Length == 1
                ).First();
    }
}