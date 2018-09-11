using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Serialization
{
    /// <summary>
    /// Default ISerializationRegistry implementation
    /// </summary>
    public class SerializationRegistry : ISerializationRegistry
    {
        static readonly ConcurrentDictionary<Type, object> _registry = new ConcurrentDictionary<Type, object>();

        ILogger _log;

        /// <summary>
        /// /
        /// </summary>
        /// <param name="log"></param>
        public SerializationRegistry(ILogger<SerializationRegistry> log)
        {
            _log = log;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEntitySerializer<T> GetSerializer<T>() where T : new() => _registry.GetValueOrDefault(typeof(T), null) as IEntitySerializer<T>;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsSerializerRegisterred<T>() where T : new() => _registry.ContainsKey(typeof(T));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializer"></param>
        /// <typeparam name="T"></typeparam>
        public void Register<T>(IEntitySerializer<T> serializer) where T : new()
        {
            _registry.AddOrUpdate(typeof(T), serializer, (t, old) =>
            {
                _log.LogWarning("Overriding existing entity serializer for {0}", typeof(T));
                return serializer;
            });
        }
    }
}