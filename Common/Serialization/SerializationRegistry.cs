using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Serialization
{
    /// <summary>
    /// Default ISerializationRegistry implementation
    /// </summary>
    public class SerializationRegistry : ISerializationRegistry
    {
        ILogger _log;
        ConcurrentDictionary<Type, object> _registry;

        /// <summary>
        /// /
        /// </summary>
        /// <param name="log"></param>
        public SerializationRegistry(ILogger<SerializationRegistry> log)
        {
            _log = log;
            _registry = new ConcurrentDictionary<Type, object>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEntitySerializer<T> GetSerializer<T>() where T : new() => (IEntitySerializer<T>)_registry.GetValueOrDefault(typeof(T), new EntitySerializer<T>());

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