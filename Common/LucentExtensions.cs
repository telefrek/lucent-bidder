using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using Lucent.Common;
using Lucent.Common.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Prometheus;

/// <summary>
/// Contains global extension methods that are useful in a variety of scenarios and not scoped to a single
/// package or utility
/// </summary>
public static partial class LucentExtensions
{
    static MD5 _md5 = System.Security.Cryptography.MD5.Create();
    static double _t2ms = 1000d / Stopwatch.Frequency;
    static double _t2s = 1d / Stopwatch.Frequency;

    /// <summary>
    /// Get the timer milliseconds
    /// </summary>
    /// <param name="timer"></param>
    /// <returns></returns>
    public static double GetMilliseconds(this Stopwatch timer) => timer.ElapsedTicks * _t2ms;

    /// <summary>
    /// Get the timer milliseconds
    /// </summary>
    /// <param name="timer"></param>
    /// <returns></returns>
    public static double GetSeconds(this Stopwatch timer) => timer.ElapsedTicks * _t2s;

    /// <summary>
    /// Calculate the hash of an buffer
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static string CalculateETag(this byte[] buffer) => _md5.ComputeHash(buffer).ToHex();

    /// <summary>
    /// Create a filter for the type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="filterType"></param>
    /// <param name="property"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Filter CreateFilter(this Type type, FilterType filterType, string property, FilterValue value) => new Filter
    {
        FilterType = filterType,
        Property = property,
        PropertyType = type.GetProperty(property).PropertyType,
        Value = value,
    };

    /// <summary>
    /// Create a filter for the type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="filterType"></param>
    /// <param name="property"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public static Filter CreateFilter(this Type type, FilterType filterType, string property, FilterValue[] values) => new Filter
    {
        FilterType = filterType,
        Property = property,
        PropertyType = type.GetProperty(property).PropertyType,
        Values = values,
    };

    /// <summary>
    /// Encode the value with URL safe characters
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string SafeBase64Encode(this string str) => str.Replace("/", "_").Replace("+", "-");

    /// <summary>
    /// Decode the safe UR encoding to valid base64
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string SafeBase64Decode(this string str) => str.Replace("_", "/").Replace("-", "+");

    /// <summary>
    /// Escape the string for XML safety
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static string Escape(this string src) => SecurityElement.Escape(src);

    /// <summary>
    /// Escape the value as a CDATA xml block
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static string CDataEscape(this string src) => "<![CDATA[" + SecurityElement.Escape(src) + "]]>";

    /// <summary>
    /// Wrap the value in a CDATA xml block
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static string CDataWrap(this string src) => "<![CDATA[" + src + "]]>";

    /// <summary>
    /// Encodes the GUID as a compressed 22 character representation
    /// </summary>
    /// <param name="g"></param>
    /// <returns></returns>
    public static string EncodeGuid(this Guid g) => Convert.ToBase64String(g.ToByteArray()).SafeBase64Encode().Substring(0, 22);

    /// <summary>
    /// Decode a compressed GUID from it's 22 character representation
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static Guid DecodeGuid(this string s) => new Guid(Convert.FromBase64String(s.SafeBase64Decode() + "=="));

    /// <summary>
    /// Hex encode a byte array
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string ToHex(this byte[] bytes)
    {
        char[] c = new char[bytes.Length * 2];

        byte b;

        for (int bx = 0, cx = 0; bx < bytes.Length; ++bx, ++cx)
        {
            b = ((byte)(bytes[bx] >> 4));
            c[cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);

            b = ((byte)(bytes[bx] & 0x0F));
            c[++cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);
        }

        return new string(c);
    }

    static int[] MSBDeBruijnLookup = new int[]
        {
            0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30,
            8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31
        };

    static int[] LSBDeBruijnLookup = new int[]
    {
            0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
            31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
    };

    /// <summary>
    /// Uses a DeBruijn Lookup to calculate the MSB.
    /// </summary>
    /// <param name="x">The value to calculate the MSB for.</param>
    /// <returns>The position of the highest set bit.</returns>
    public static int MSB(this int x)
    {
        x |= x >> 1;
        x |= x >> 2;
        x |= x >> 4;
        x |= x >> 8;
        x |= x >> 16;

        return MSBDeBruijnLookup[(uint)(x * 0x07C4ACDDU) >> 27];
    }

    /// <summary>
    /// Uses a DeBruijn Lookup to calculate the LSB.
    /// </summary>
    /// <param name="x">The value to calculate the LSB for.</param>
    /// <returns>The position of the lowest set bit.</returns>
    public static int LSB(this int x)
    {
        return LSBDeBruijnLookup[(uint)((x & -x) * 0x077CB531U) >> 27];
    }

    /// <summary>
    /// Format a string in place
    /// </summary>
    /// <param name="format">The format string to use</param>
    /// <param name="data">The data to substitute into the format</param>
    /// <returns>A formatted string</returns>
    public static string FormatWith(this string format, params object[] data) => string.Format(format, data);

    /// <summary>
    /// URL Encode the string
    /// </summary>
    /// <param name="original"></param>
    /// <returns></returns>
    public static string UrlEncode(this string original) => WebUtility.UrlEncode(original);

    /// <summary>
    /// URL Decode the string
    /// </summary>
    /// <param name="encoded"></param>
    /// <returns></returns>
    public static string UrlDecode(this string encoded) => WebUtility.UrlDecode(encoded);

    /// <summary>
    /// Sets the value in the dictionary, regardless of whether or not it exists
    /// </summary>
    /// <param name="dictionary">The dictionary to modify</param>
    /// <param name="key">The key to search with</param>
    /// <param name="value">The value to upsert</param>
    /// <typeparam name="T">The key type</typeparam>
    /// <typeparam name="F">The value type</typeparam>
    public static void Set<T, F>(this IDictionary<T, F> dictionary, T key, F value)
    {
        // I just hate typing this all the time honestly...
        if (dictionary.ContainsKey(key))
            dictionary[key] = value;
        else
            dictionary.Add(key, value);
    }

    /// <summary>
    /// Creates an instance of the given type, injecting parameters where possible from the provider
    /// 
    /// Throws a LucentException
    /// </summary>
    /// <param name="provider">The provider to use for parameter resolution</param>
    /// <param name="supplied">The supplied parameters</param>
    /// <typeparam name="T">The type of object to craete</typeparam>
    /// <exception cref="Lucent.Common.LucentException">If there is a problem resolving the object</exception>
    /// <returns>A new instance of the object if it can be created</returns>
    public static T CreateInstance<T>(this IServiceProvider provider, params object[] supplied)
        => (T)provider.CreateInstance(typeof(T), supplied);

    /// <summary>
    /// Creates an instance of the given type, injecting parameters where possible from the provider
    /// 
    /// Throws a LucentException
    /// </summary>
    /// <param name="provider">The provider to use for parameter resolution</param>
    /// <param name="t"></param>
    /// <param name="supplied">The supplied parameters</param>
    /// <exception cref="Lucent.Common.LucentException">If there is a problem resolving the object</exception>
    /// <returns>A new instance of the object if it can be created</returns>
    public static object CreateInstance(this IServiceProvider provider, Type t, params object[] supplied)
    {
        var types = supplied.Select(s => s.GetType()).ToArray();
        var cinfo = t.GetConstructors().Where(c => IsMatch(c, types)).FirstOrDefault();

        if (cinfo != null)
        {
            try
            {
                // Basically the same loop as match test but with calls to provider to resolve missing parameters
                var pArr = cinfo.GetParameters();
                var pMap = new object[pArr.Length];

                // Fill the supplied and parameter mixed
                var idx = 0;
                var i = 0;
                for (; i < pArr.Length && idx < types.Length; ++i)
                    if (pArr[i].ParameterType.IsAssignableFrom(types[idx]))
                        pMap[i] = supplied[idx++];
                    else
                        pMap[i] = provider.GetService(pArr[i].ParameterType);

                // Finish any remaining injections
                for (; i < pArr.Length; ++i)
                    pMap[i] = provider.GetService(pArr[i].ParameterType);

                // Hope for the best
                return cinfo.Invoke(pMap);
            }
            catch (Exception e)
            {
                // Wrap and throw it
                throw new LucentException("Failed to create object", e);
            }
        }

        // Boom goes the dynamite!
        throw new LucentException("Failed to locate constructor to match parameters supplied with provider");
    }

    /// <summary>
    /// Test to validate if it's possible to use this constructor with the supplied types
    /// 
    /// NOTE: This is really not safe, but a convenient hack for this project
    /// </summary>
    /// <param name="constructorInfo">The constructor to test</param>
    /// <param name="suppliedTypes">The types supplied already</param>
    /// <returns>True if the constructor is a match for creating</returns>
    static bool IsMatch(ConstructorInfo constructorInfo, Type[] suppliedTypes)
    {
        try
        {
            // Filter out non-public and static constructors
            if (!constructorInfo.IsPublic) return false;
            if (constructorInfo.IsStatic) return false;

            // Get the parameters
            var pArr = constructorInfo.GetParameters();

            // Can't use one with less than the supplied
            if (pArr.Length < suppliedTypes.Length) return false;

            // Supplied types must be in order, interfaces ASSUMED to be in service provider if not matched...
            var idx = 0;
            var i = 0;
            for (i = 0; i < pArr.Length && idx < suppliedTypes.Length; ++i)
            {
                if (pArr[i].ParameterType.IsAssignableFrom(suppliedTypes[idx]))
                    idx++;
                else if (!pArr[i].ParameterType.IsInterface)
                    break;
            }

            // May have residual types left over
            for (; i < pArr.Length; ++i)
                if (!pArr[i].ParameterType.IsInterface)
                    break;

            // Have to make it through both arrays for this to be true
            return idx == suppliedTypes.Length && i == pArr.Length;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Add status codes and Api latency tracking
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseApiLatency(this IApplicationBuilder app)
    {
        // Track the api latency for each request type
        var api_latency = Metrics.CreateHistogram("api_latency", "Latency for api calls", new HistogramConfiguration
        {
            LabelNames = new string[] { "method", "path" },
            Buckets = new double[] { 0.005, 0.010, 0.015, 0.025, 0.050, 0.075, 0.100, 0.125, 0.150, 0.200, 0.25, 0.5, 0.75, 1.0 },
        });

        var status_code = Metrics.CreateCounter("status_codes", "Status codes", new CounterConfiguration
        {
            LabelNames = new string[] { "method", "path", "status" },
        });

        // This should be fun...
        app.Use(async (context, next) =>
        {
            var instance = api_latency.WithLabels(context.Request.Method, context.Request.Path);
            var sw = Stopwatch.StartNew();
            await next().ContinueWith(t =>
            {
                instance.Observe(sw.ElapsedTicks * 1000d / Stopwatch.Frequency);
                status_code.WithLabels(context.Request.Method, context.Request.Path, context.Response.StatusCode.ToString()).Inc();
            });
        });

        return app;
    }
}