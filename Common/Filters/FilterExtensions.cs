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
        /// <param name="bidTarget"></param>
        /// <returns></returns>
        public static Func<BidRequest, bool> GenerateTargets(this BidTargets bidTarget)
        {
            // Need our input parameter :)
            var bidParam = Expression.Parameter(typeof(BidRequest), "bid");

            // Need a variable to track the complex filtering
            var fValue = Expression.Variable(typeof(bool), "isTargetted");
            var gValue = Expression.Variable(typeof(int), "geoMatch");

            // Keep track of all the expressions in this chain
            var expList = new List<Expression> { };
            expList.Add(Expression.Assign(gValue, Expression.Constant(2)));

            // Need a sentinal value for breaking in loops
            var loopBreak = Expression.Label();
            var ret = Expression.Label(typeof(bool)); // We're going to return a bool

            // Process the impressions filters
            if (bidTarget.ImpressionTargets != null)
            {
                // Get the impressions
                var impProp = Expression.Property(bidParam, "Impressions");
                var impType = typeof(Impression);

                // Create a loop parameter and the filter
                var impParam = Expression.Parameter(impType, "imp");
                var impTest = CombineTargets(bidTarget.ImpressionTargets, impParam);

                // Create the loop and add it to this block
                expList.Add(
                    Expression.IfThenElse(
                        Expression.NotEqual(impProp, Expression.Constant(null)),
                        ForEach(impProp, impParam,
                            Expression.IfThen(
                                Expression.OrElse(
                                    Expression.Equal(impParam, Expression.Constant(null)),
                                    Expression.Not(impTest)),
                                    Expression.Return(ret, Expression.Constant(false)))),
                        Expression.Return(ret, Expression.Constant(false))));
            }

            // Process the user filters
            if (bidTarget.DeviceTargets != null || bidTarget.GeoTargets != null)
            {
                // Get the user property and filters
                var deviceProp = Expression.Property(bidParam, "Device");
                var deviceFilter = CombineTargets(bidTarget.DeviceTargets, deviceProp);

                // Get the geographical filter and property
                var geoProp = Expression.Property(deviceProp, "Geo");
                var geoFilter = (Expression)Expression.Constant(bidTarget.GeoTargets == null);
                if (bidTarget.GeoTargets != null)
                    geoFilter = CombineTargets(bidTarget.GeoTargets, geoProp);

                if (deviceFilter != null)
                    expList.Add(
                        Expression.IfThen(
                            Expression.OrElse(
                                Expression.Equal(deviceProp, Expression.Constant(null)),
                                Expression.Not(deviceFilter)
                            ),
                            Expression.Return(ret, Expression.Constant(false))
                        )
                    );

                if (geoFilter != null)
                    expList.Add(
                        Expression.IfThen(
                            Expression.OrElse(
                                Expression.OrElse(
                                    Expression.Equal(deviceProp, Expression.Constant(null)),
                                    Expression.Equal(geoProp, Expression.Constant(null))),
                                Expression.Not(geoFilter)
                            ),
                            Expression.Assign(gValue, Expression.Decrement(gValue))
                        )
                    );
            }

            // Process the user filters
            if (bidTarget.UserTargets != null || bidTarget.GeoTargets != null)
            {
                // Get the user property and filters
                var userProp = Expression.Property(bidParam, "User");
                var userFilter = CombineTargets(bidTarget.UserTargets, userProp);

                // Get the geographical filter and property
                var geoProp = Expression.Property(userProp, "Geo");
                var geoFilter = (Expression)Expression.Constant(bidTarget.GeoTargets == null);
                if (bidTarget.GeoTargets != null)
                    geoFilter = CombineTargets(bidTarget.GeoTargets, geoProp);

                if (userFilter != null)
                    expList.Add(
                        Expression.IfThen(
                            Expression.OrElse(
                                Expression.Equal(userProp, Expression.Constant(null)),
                                Expression.Not(userFilter)
                            ),
                            Expression.Return(ret, Expression.Constant(false))
                        )
                    );

                if (geoFilter != null)
                    expList.Add(
                        Expression.IfThen(
                            Expression.OrElse(
                                Expression.OrElse(
                                    Expression.Equal(userProp, Expression.Constant(null)),
                                    Expression.Equal(geoProp, Expression.Constant(null))),
                                Expression.Not(geoFilter)
                            ),
                            Expression.Assign(gValue, Expression.Decrement(gValue))
                        )
                    );
            }

            // Geo check
            expList.Add(Expression.IfThen(Expression.Equal(gValue, Expression.Constant(0)),
                Expression.Return(ret, Expression.Constant(false))));

            // Process the site filters
            if (bidTarget.SiteTargets != null)
            {
                // Get the impressions
                var siteProp = Expression.Property(bidParam, "Site");
                var siteTest = CombineTargets(bidTarget.SiteTargets, siteProp);

                // Create the loop and add it to this block
                if (siteTest != null)
                    expList.Add(
                        Expression.IfThen(
                            Expression.OrElse(
                                Expression.Equal(siteProp, Expression.Constant(null)),
                                Expression.Not(siteTest)
                            ),
                            Expression.Return(ret, Expression.Constant(false))
                        )
                    );
            }

            // Process the app filters
            if (bidTarget.AppTargets != null)
            {
                // Get the impressions
                var appProp = Expression.Property(bidParam, "App");
                var appTest = CombineTargets(bidTarget.SiteTargets, appProp);

                // Create the loop and add it to this block
                if (appTest != null)
                    expList.Add(
                        Expression.IfThen(
                            Expression.OrElse(
                                Expression.Equal(appProp, Expression.Constant(null)),
                                Expression.Not(appTest)
                            ),
                            Expression.Return(ret, Expression.Constant(false))
                        )
                    );
            }

            // If you make it through all targets, you pass
            expList.Add(Expression.Label(ret, Expression.Constant(true)));

            var final = Expression.Block(new ParameterExpression[] { gValue }, expList);

            var ftype = typeof(Func<,>).MakeGenericType(typeof(BidRequest), typeof(bool));
            var comp = makeLambda.MakeGenericMethod(ftype).Invoke(null, new object[] { final, new ParameterExpression[] { bidParam } });
            return (Func<BidRequest, bool>)comp.GetType().GetMethod("Compile", Type.EmptyTypes).Invoke(comp, new object[] { });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonFilters"></param>
        /// <param name="original"></param>
        /// <returns></returns>
        public static BidFilter MergeFilter(this JsonFilter[] jsonFilters, BidFilter original)
        {
            original = original ?? new BidFilter();
            foreach (var jsonObj in jsonFilters ?? new JsonFilter[0])
            {
                var filter = new Filter
                {
                    FilterType = Enum.Parse<FilterType>(jsonObj.Operation, true),
                    Value = jsonObj.Values.Length == 1 ? jsonObj.Values.Last() : null,
                    Values = jsonObj.Values.Length > 1 ? jsonObj.Values : null,
                };

                switch (jsonObj.Entity)
                {
                    case "geo":
                        if (TryParseProperty<Geo>(filter, jsonObj))
                        {
                            var filters = original.GeoFilters ?? new Filter[0];
                            Array.Resize(ref filters, filters.Length + 1);
                            filters[filters.Length - 1] = filter;
                            original.GeoFilters = filters;
                        }
                        break;
                    case "device":
                        if (TryParseProperty<Device>(filter, jsonObj))
                        {
                            var filters = original.DeviceFilters ?? new Filter[0];
                            Array.Resize(ref filters, filters.Length + 1);
                            filters[filters.Length - 1] = filter;
                            original.DeviceFilters = filters;
                        }
                        break;
                    case "app":
                        if (TryParseProperty<App>(filter, jsonObj))
                        {
                            var filters = original.AppFilters ?? new Filter[0];
                            Array.Resize(ref filters, filters.Length + 1);
                            filters[filters.Length - 1] = filter;
                            original.AppFilters = filters;
                        }
                        break;
                    case "site":
                        if (TryParseProperty<Site>(filter, jsonObj))
                        {
                            var filters = original.SiteFilters ?? new Filter[0];
                            Array.Resize(ref filters, filters.Length + 1);
                            filters[filters.Length - 1] = filter;
                            original.SiteFilters = filters;
                        }
                        break;
                    case "impression":
                        if (TryParseProperty<Impression>(filter, jsonObj))
                        {
                            var filters = original.ImpressionFilters ?? new Filter[0];
                            Array.Resize(ref filters, filters.Length + 1);
                            filters[filters.Length - 1] = filter;
                            original.ImpressionFilters = filters;
                        }
                        break;
                    case "user":
                        if (TryParseProperty<User>(filter, jsonObj))
                        {
                            var filters = original.UserFilters ?? new Filter[0];
                            Array.Resize(ref filters, filters.Length + 1);
                            filters[filters.Length - 1] = filter;
                            original.UserFilters = filters;
                        }
                        break;
                }
            }

            return original;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonFilters"></param>
        /// <param name="original"></param>
        /// <returns></returns>
        public static BidTargets MergeTarget(this JsonFilter[] jsonFilters, BidTargets original)
        {
            original = original ?? new BidTargets();
            foreach (var jsonObj in jsonFilters ?? new JsonFilter[0])
            {
                var filter = new Target
                {
                    TargetType = Enum.Parse<FilterType>(jsonObj.Operation, true),
                    Value = jsonObj.Values.Length == 1 ? jsonObj.Values.Last() : null,
                    Values = jsonObj.Values.Length > 1 ? jsonObj.Values : null,
                };

                switch (jsonObj.Entity)
                {
                    case "geo":
                        if (TryParseProperty<Geo>(filter, jsonObj))
                        {
                            var filters = original.GeoTargets ?? new Target[0];
                            Array.Resize(ref filters, filters.Length + 1);
                            filters[filters.Length - 1] = filter;
                            original.GeoTargets = filters;
                        }
                        break;
                    case "device":
                        if (TryParseProperty<Device>(filter, jsonObj))
                        {
                            var filters = original.DeviceTargets ?? new Target[0];
                            Array.Resize(ref filters, filters.Length + 1);
                            filters[filters.Length - 1] = filter;
                            original.DeviceTargets = filters;
                        }
                        break;
                    case "app":
                        if (TryParseProperty<App>(filter, jsonObj))
                        {
                            var filters = original.AppTargets ?? new Target[0];
                            Array.Resize(ref filters, filters.Length + 1);
                            filters[filters.Length - 1] = filter;
                            original.AppTargets = filters;
                        }
                        break;
                    case "site":
                        if (TryParseProperty<Site>(filter, jsonObj))
                        {
                            var filters = original.SiteTargets ?? new Target[0];
                            Array.Resize(ref filters, filters.Length + 1);
                            filters[filters.Length - 1] = filter;
                            original.SiteTargets = filters;
                        }
                        break;
                    case "impression":
                        if (TryParseProperty<Impression>(filter, jsonObj))
                        {
                            var filters = original.ImpressionTargets ?? new Target[0];
                            Array.Resize(ref filters, filters.Length + 1);
                            filters[filters.Length - 1] = filter;
                            original.ImpressionTargets = filters;
                        }
                        break;
                    case "user":
                        if (TryParseProperty<User>(filter, jsonObj))
                        {
                            var filters = original.UserTargets ?? new Target[0];
                            Array.Resize(ref filters, filters.Length + 1);
                            filters[filters.Length - 1] = filter;
                            original.UserTargets = filters;
                        }
                        break;
                }
            }
            
            return original;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="jsonObj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        static bool TryParseProperty<T>(Filter filter, JsonFilter jsonObj)
        {
            var property = jsonObj.Property;
            switch (property.ToLower())
            {
                case "ispaid":
                    property = "ispaidversion";
                    break;
                case "appcategory":
                    property = "appcategories";
                    break;
                case "sectioncategory":
                    property = "sectioncategories";
                    break;
                case "pagecategory":
                    property = "pagecategories";
                    break;
                case "sitecategory":
                    property = "sitecategories";
                    break;
                case "issecure":
                    property = "IsHttpsRequired";
                    break;
                case "os_version":
                    property = "osversion";
                    break;
                case "type":
                    property = "geotype";
                    break;
            }

            var prop = typeof(T).GetProperty(property, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
            if (prop != null)
            {
                filter.Property = prop.Name;
                filter.PropertyType = prop.PropertyType;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="jsonObj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        static bool TryParseProperty<T>(Target filter, JsonFilter jsonObj)
        {
            var property = jsonObj.Property;
            switch (property.ToLower())
            {
                case "ispaid":
                    property = "ispaidversion";
                    break;
                case "appcategory":
                    property = "appcategories";
                    break;
                case "sectioncategory":
                    property = "sectioncategories";
                    break;
                case "pagecategory":
                    property = "pagecategories";
                    break;
                case "sitecategory":
                    property = "sitecategories";
                    break;
                case "issecure":
                    property = "IsHttpsRequired";
                    break;
                case "os_version":
                    property = "osversion";
                    break;
                case "type":
                    property = "geotype";
                    break;
            }

            var prop = typeof(T).GetProperty(property, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
            if (prop != null)
            {
                filter.Property = prop.Name;
                filter.PropertyType = prop.PropertyType;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bidFilter"></param>
        /// <returns></returns>
        public static Func<BidRequest, bool> GenerateFilter(this BidFilter bidFilter)
        {
            // Need our input parameter :)
            var bidParam = Expression.Parameter(typeof(BidRequest), "bid");

            // Need a variable to track the complex filtering
            var fValue = Expression.Variable(typeof(bool), "isTargetted");
            var gValue = Expression.Variable(typeof(int), "geoMatch");

            // Keep track of all the expressions in this chain
            var expList = new List<Expression> { };
            expList.Add(Expression.Assign(gValue, Expression.Constant(2)));

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
                expList.Add(
                    Expression.IfThenElse(
                        Expression.NotEqual(impProp, Expression.Constant(null)),
                        ForEach(impProp, impParam,
                            Expression.IfThen(
                                    impTest,
                                    Expression.Return(ret, Expression.Constant(true)))),
                        Expression.Return(ret, Expression.Constant(true))));
            }

            // Process the user filters
            if (bidFilter.DeviceFilters != null || bidFilter.GeoFilters != null)
            {
                // Get the user property and filters
                var deviceProp = Expression.Property(bidParam, "Device");
                var deviceFilter = CombineFilters(bidFilter.DeviceFilters, deviceProp);

                // Get the geographical filter and property
                var geoProp = Expression.Property(deviceProp, "Geo");
                var geoFilter = (Expression)Expression.Constant(bidFilter.GeoFilters == null);
                if (bidFilter.GeoFilters != null)
                    geoFilter = CombineFilters(bidFilter.GeoFilters, geoProp);

                if (deviceFilter != null)
                    expList.Add(
                        Expression.IfThen(
                            Expression.AndAlso(
                                Expression.NotEqual(deviceProp, Expression.Constant(null)),
                                deviceFilter
                            ),
                            Expression.Return(ret, Expression.Constant(true)))
                    );

                if (geoFilter != null)
                    expList.Add(
                        Expression.IfThen(
                            Expression.AndAlso(
                                Expression.Not(Expression.OrElse(
                                    Expression.Equal(deviceProp, Expression.Constant(null)),
                                    Expression.Equal(geoProp, Expression.Constant(null)))),
                                geoFilter
                            ),
                            Expression.Assign(gValue, Expression.Decrement(gValue))
                        )
                    );
            }

            // Process the user filters
            if (bidFilter.UserFilters != null || bidFilter.GeoFilters != null)
            {
                // Get the user property and filters
                var userProp = Expression.Property(bidParam, "User");
                var userFilter = CombineFilters(bidFilter.UserFilters, userProp);

                // Get the geographical filter and property
                var geoProp = Expression.Property(userProp, "Geo");
                var geoFilter = (Expression)Expression.Constant(bidFilter.GeoFilters == null);
                if (bidFilter.GeoFilters != null)
                    geoFilter = CombineFilters(bidFilter.GeoFilters, geoProp);

                if (userFilter != null)
                    expList.Add(
                        Expression.IfThen(
                            Expression.AndAlso(
                                Expression.NotEqual(userProp, Expression.Constant(null)),
                                userFilter
                            ),
                            Expression.Return(ret, Expression.Constant(true))
                        )
                    );

                if (geoFilter != null)
                    expList.Add(
                        Expression.IfThen(
                            Expression.AndAlso(
                                Expression.Not(Expression.OrElse(
                                    Expression.Equal(userProp, Expression.Constant(null)),
                                    Expression.Equal(geoProp, Expression.Constant(null)))),
                                geoFilter
                            ),
                            Expression.Assign(gValue, Expression.Decrement(gValue))
                        )
                    );
            }

            // Geo check
            expList.Add(Expression.IfThen(Expression.LessThan(gValue, Expression.Constant(2)),
                Expression.Return(ret, Expression.Constant(true))));

            // Process the site filters
            if (bidFilter.SiteFilters != null)
            {
                // Get the impressions
                var siteProp = Expression.Property(bidParam, "Site");
                var siteTest = CombineFilters(bidFilter.SiteFilters, siteProp);

                // Create the loop and add it to this block
                if (siteTest != null)
                    expList.Add(
                        Expression.IfThen(
                            Expression.AndAlso(
                                Expression.NotEqual(siteProp, Expression.Constant(null)),
                                siteTest
                            ),
                            Expression.Return(ret, Expression.Constant(true))
                        )
                    );
            }

            // Process the app filters
            if (bidFilter.AppFilters != null)
            {
                // Get the impressions
                var appProp = Expression.Property(bidParam, "App");
                var appTest = CombineFilters(bidFilter.SiteFilters, appProp);

                // Create the loop and add it to this block
                if (appTest != null)
                    expList.Add(
                        Expression.IfThen(
                            Expression.AndAlso(
                                Expression.NotEqual(appProp, Expression.Constant(null)),
                                appTest
                            ),
                            Expression.Return(ret, Expression.Constant(true))
                        )
                    );
            }

            // If you make it through all filters, you pass
            expList.Add(Expression.Label(ret, Expression.Constant(false)));

            var final = Expression.Block(new ParameterExpression[] { gValue }, expList);

            var ftype = typeof(Func<,>).MakeGenericType(typeof(BidRequest), typeof(bool));
            var comp = makeLambda.MakeGenericMethod(ftype).Invoke(null, new object[] { final, new ParameterExpression[] { bidParam } });
            return (Func<BidRequest, bool>)comp.GetType().GetMethod("Compile", Type.EmptyTypes).Invoke(comp, new object[] { });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Expression CreateExpression(this Target target, Expression p)
        {
            var prop = Expression.Property(p, target.Property);
            var ptype = (prop.Member as PropertyInfo).PropertyType;

            Expression exp = Expression.Constant(true);
            var fExpVal = Expression.Constant(typeof(LucentExtensions).GetMethods()
                                            .Single(m => m.IsGenericMethod && m.Name == "CastTo").MakeGenericMethod(ptype.IsArray ? ptype.GetElementType() : ptype).Invoke(null, new object[] { target.Value }));

            var fValsExp = Expression.Constant(typeof(LucentExtensions).GetMethods()
                                            .Single(m => m.IsGenericMethod && m.Name == "CastArrayTo").MakeGenericMethod(ptype.IsArray ? ptype.GetElementType() : ptype).Invoke(null, new object[] { target.Values }));
            switch (target.TargetType)
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
                case FilterType.HASVALUE:
                    exp = Expression.Not(Expression.Call(null, typeof(LucentExtensions).GetMethods().Single(m => m.Name.Equals("IsNullOrDefault") && m.IsGenericMethod && m.GetParameters().Length == 1).MakeGenericMethod(ptype), prop));
                    break;
                case FilterType.IN:
                case FilterType.NOTIN:
                    if (ptype.IsArray)
                    {
                        if (target.Values != null)
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
                        else if (target.Value != null)
                        {
                            exp = Expression.Call(null,
                                typeof(Enumerable).GetMethods().Single(m => m.Name.Equals("Contains") && m.IsGenericMethod && m.GetParameters().Length == 2)
                                    .MakeGenericMethod(ptype.GetElementType()), prop,
                                        Expression.Convert(fExpVal, ptype.GetElementType()));
                        }
                    }
                    else if (ptype.IsAssignableFrom(typeof(string)))
                    {
                        if (target.Values != null)
                        {
                            var lamRet = Expression.Label(typeof(bool));
                            var lamParam = Expression.Parameter(typeof(string), "s");
                            var expList = new List<Expression>();
                            expList.Add(Expression.IfThen(Expression.Call(prop, typeof(string).GetMethods().First(m => m.Name == "Contains" && m.GetParameters().Length == 2 && m.GetParameters().First().ParameterType == typeof(string)), lamParam, Expression.Constant(StringComparison.InvariantCultureIgnoreCase)), Expression.Return(lamRet, Expression.Constant(true))));

                            expList.Add(Expression.Label(lamRet, Expression.Constant(false)));

                            var final = Expression.Block(expList);
                            var ftype = typeof(Func<,>).MakeGenericType(typeof(string), typeof(bool));

                            exp = Expression.Call(null, typeof(Enumerable).GetMethods().Single(m => m.Name.Equals("Any") && m.IsGenericMethod && m.GetParameters().Length == 2).MakeGenericMethod(typeof(string)), fValsExp, Expression.Lambda(final, false, new ParameterExpression[] { lamParam }));

                        }
                        else if (target.Value != null)
                        {
                            exp = Expression.Call(prop, typeof(string).GetMethods().First(m => m.Name == "Contains" && m.GetParameters().Length == 2 && m.GetParameters().First().ParameterType == typeof(string)), fExpVal, Expression.Constant(StringComparison.InvariantCultureIgnoreCase));
                        }
                    }
                    else if (ptype.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)) != null)
                    {
                        // This never gets used right now, depends on definition
                        if (target.Values != null)
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
                        else if (target.Value != null)
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
                    if (target.TargetType == FilterType.NOTIN)
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
                case FilterType.HASVALUE:
                    exp = Expression.Not(Expression.Call(null, typeof(LucentExtensions).GetMethods().Single(m => m.Name.Equals("IsNullOrDefault") && m.IsGenericMethod && m.GetParameters().Length == 1).MakeGenericMethod(ptype), prop));
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
                            expList.Add(Expression.IfThen(Expression.Call(prop, typeof(string).GetMethods().First(m => m.Name == "Contains" && m.GetParameters().Length == 2 && m.GetParameters().First().ParameterType == typeof(string)), lamParam, Expression.Constant(StringComparison.InvariantCultureIgnoreCase)), Expression.Return(lamRet, Expression.Constant(true))));

                            expList.Add(Expression.Label(lamRet, Expression.Constant(false)));

                            var final = Expression.Block(expList);
                            var ftype = typeof(Func<,>).MakeGenericType(typeof(string), typeof(bool));

                            exp = Expression.Call(null, typeof(Enumerable).GetMethods().Single(m => m.Name.Equals("Any") && m.IsGenericMethod && m.GetParameters().Length == 2).MakeGenericMethod(typeof(string)), fValsExp, Expression.Lambda(final, false, new ParameterExpression[] { lamParam }));

                        }
                        else if (filter.Value != null)
                        {
                            exp = Expression.Call(prop, typeof(string).GetMethods().First(m => m.Name == "Contains" && m.GetParameters().Length == 2 && m.GetParameters().First().ParameterType == typeof(string)), fExpVal, Expression.Constant(StringComparison.InvariantCultureIgnoreCase));
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

            foreach (var filter in filters ?? new Filter[] { })
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
        /// 
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Expression CombineTargets(this ICollection<Target> targets, Expression target)
        {
            Expression exp = null;

            foreach (var t in targets ?? new List<Target>())
            {
                var e = t.CreateExpression(target);

                if (exp == null)
                    exp = e;
                else
                    exp = Expression.AndAlso(exp, e);
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