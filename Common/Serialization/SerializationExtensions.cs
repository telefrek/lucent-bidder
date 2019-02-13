using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Lucent.Common.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Lucent.Common
{
    /// <summary>
    /// Serialization extension methods
    /// </summary>
    public static partial class LucentExtensions
    {
        /// <summary>
        /// Get a reader from the current context
        /// </summary>
        /// <param name="serializationContext"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static async Task<T> ReadAs<T>(this ISerializationContext serializationContext, HttpContext httpContext) where T : class, new()
        {
            using (var body = httpContext.Request.Body)
            {
                var format = (httpContext.Request.ContentType ?? "").Contains("protobuf") ? SerializationFormat.PROTOBUF : SerializationFormat.JSON;

                var encoding = StringValues.Empty;
                if (httpContext.Request.Headers.TryGetValue("Content-Encoding", out encoding))
                    if (encoding.Any(e => e.Contains("gzip")))
                        format |= SerializationFormat.COMPRESSED;

                return await serializationContext.ReadFrom<T>(body, false, format);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static async Task WriteTo<T>(this ISerializationContext serializationContext, HttpContext httpContext, ICollection<T> instance) where T : class, new()
        {
            var format = (httpContext.Request.ContentType ?? "").Contains("protobuf") ? SerializationFormat.PROTOBUF : SerializationFormat.JSON;

            // Write the response encoding
            switch (format)
            {
                case SerializationFormat.PROTOBUF:
                    httpContext.Response.ContentType = "application/x-protobuf";
                    break;
                default:
                    httpContext.Response.ContentType = "application/json";
                    break;
            }

            var encoding = StringValues.Empty;
            if (httpContext.Request.Headers.TryGetValue("Accept-Encoding", out encoding))
                if (encoding.Any(e => e.Contains("gzip")))
                    format |= SerializationFormat.COMPRESSED;

            await serializationContext.WriteTo(instance, httpContext.Response.Body, false, format);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static async Task WriteTo<T>(this ISerializationContext serializationContext, HttpContext httpContext, T instance) where T : class, new()
        {
            var format = (httpContext.Request.ContentType ?? "").Contains("protobuf") ? SerializationFormat.PROTOBUF : SerializationFormat.JSON;

            // Write the response encoding
            switch (format)
            {
                case SerializationFormat.PROTOBUF:
                    httpContext.Response.ContentType = "application/x-protobuf";
                    break;
                default:
                    httpContext.Response.ContentType = "application/json";
                    break;
            }

            var encoding = StringValues.Empty;
            if (httpContext.Request.Headers.TryGetValue("Accept-Encoding", out encoding))
                if (encoding.Any(e => e.Contains("gzip")))
                    format |= SerializationFormat.COMPRESSED;

            await serializationContext.WriteTo(instance, httpContext.Response.Body, false, format);
        }

        /// <summary>
        /// 
        /// </summary>
        public async static Task<HttpResponseMessage> PostJsonAsync<T>(this HttpClient httpClient, ISerializationContext serializationContext, T instance, string path) where T : class, new()
        {
            using (var ms = new MemoryStream())
            {
                await serializationContext.WriteTo(instance, ms, true, SerializationFormat.JSON);
                ms.Seek(0, SeekOrigin.Begin);

                using (var content = new StreamContent(ms, 4092))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    return await httpClient.PostAsync(path, content);
                }
            }
        }

         /// <summary>
        /// 
        /// </summary>
        public async static Task<HttpResponseMessage> PutJsonAsync<T>(this HttpClient httpClient, ISerializationContext serializationContext, T instance, string path) where T : class, new()
        {
            using (var ms = new MemoryStream())
            {
                await serializationContext.WriteTo(instance, ms, true, SerializationFormat.JSON);
                ms.Seek(0, SeekOrigin.Begin);

                using (var content = new StreamContent(ms, 4092))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    return await httpClient.PutAsync(path, content);
                }
            }
        }

        /// <summary>
        /// Validates if an object is the default for it's type
        /// </summary>
        /// <param name="instance">an instance of the object</param>
        /// <typeparam name="T">The type of object</typeparam>
        /// <returns></returns>
        public static bool IsNullOrDefault<T>(this T instance)
        {
            if (instance == null) return true;
            if (object.Equals(instance, default(T))) return true;

            Type methodType = typeof(T);
            if (Nullable.GetUnderlyingType(methodType) != null) return false;

            Type instanceType = instance.GetType();
            if (typeof(ICollection).IsAssignableFrom(instanceType))
            {
                var col = instance as ICollection;
                if (col == null || col.Count == 0)
                    return true;
            }

            if (instanceType.IsValueType && instanceType != methodType)
            {
                object obj = Activator.CreateInstance(instance.GetType());
                return obj.Equals(instance);
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object UnwrapValue<T>(this T instance)
        {
            Type instanceType = instance.GetType();
            if (instanceType.IsEnum)
                return Convert.ToInt32(instance);

            return instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="target"></param>
        /// <param name="value"></param>
        public static void SetWrapValue(this PropertyInfo info, object target, object value)
        {
            if (info.PropertyType.IsEnum)
            {
                foreach (var val in Enum.GetValues(info.PropertyType))
                    if (Convert.ToInt32(val).Equals(Convert.ToInt32(value)))
                    {
                        info.SetValue(target, val);
                        return;
                    }
            }
            else if (info.PropertyType.Equals(typeof(Guid)))
            {
                info.SetValue(target, Guid.Parse(value as string));
            }
            else if (info.PropertyType.Equals(typeof(DateTime)))
            {
                info.SetValue(target, DateTime.Parse(value as string));
            }
            else if (typeof(IConvertible).IsAssignableFrom(info.PropertyType))
                info.SetValue(target, Convert.ChangeType(value, info.PropertyType));
            else
                info.SetValue(target, value);
        }
    }
}