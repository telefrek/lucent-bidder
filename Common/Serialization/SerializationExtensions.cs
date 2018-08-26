using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Lucent.Common.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lucent.Common
{
    /// <summary>
    /// Serialization extension methods
    /// </summary>
    public static partial class LucentExtensions
    {
        /// <summary>
        /// Hook to configure serialization registry creation during runtime
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddSerialization(this IServiceCollection services, IConfiguration configuration) =>
            // Each request should get it's own seriailzation registry, depending on context
            services.AddSingleton<ISerializationRegistry, SerializationRegistry>();

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

        public static ISerializationStream WrapSerializer(this Stream target, IServiceProvider provider, SerializationFormat format, bool leaveOpen) => new SerializationStream(target, format, provider, leaveOpen);

        public static object UnwrapValue<T>(this T instance)
        {
            Type instanceType = instance.GetType();
            if (instanceType.IsEnum)
                return Convert.ToInt32(instance);

            return instance;
        }

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