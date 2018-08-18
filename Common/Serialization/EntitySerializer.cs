using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Lucent.Common;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Lucent.Common.Serialization
{
    /// <summary>
    /// Default serializer that uses dynamic objects and reflection
    /// </summary>
    /// <typeparam name="T">The type of object to serialize</typeparam>
    public class EntitySerializer<T> : IEntitySerializer<T>
        where T : new()
    {
        public T Read(ISerializationStreamReader serializationStreamReader)
        {
            var dObj = serializationStreamReader.ReadDynamic() as IDictionary<string, object>;
            if (dObj == null)
                return default(T);

            T instance = new T();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(pi => pi.Name.ToLowerInvariant(), pi => pi);

            foreach (var prop in dObj.Keys)
                if (props.ContainsKey(prop.ToLowerInvariant()))
                    props[prop.ToLowerInvariant()].SetWrapValue(instance, dObj[prop]);

            return instance;
        }

        public async Task<T> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            var dObj = await serializationStreamReader.ReadDynamicAsync() as IDictionary<string, object>;
            if (dObj == null)
                return default(T);

            T instance = new T();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty).ToDictionary(pi => pi.Name.ToLowerInvariant(), pi => pi);

            foreach (var prop in dObj.Keys)
                if (props.ContainsKey(prop.ToLowerInvariant()))
                    props[prop.ToLowerInvariant()].SetWrapValue(instance, dObj[prop]);

            return instance;
        }

        public void Write(ISerializationStreamWriter serializationStreamWriter, T instance)
        {
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty).ToDictionary(pi => pi.Name.ToLowerInvariant(), pi => pi);

            dynamic dObj = new ExpandoObject();
            var dProps = dObj as IDictionary<string, object>;

            foreach (var prop in props.Keys)
            {
                var val = props[prop].GetValue(instance);
                if (!val.IsNullOrDefault())
                    dProps.Add(prop, val.UnwrapValue());
            }

            serializationStreamWriter.Write(dObj);
        }

        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, T instance, CancellationToken token)
        {
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty).ToDictionary(pi => pi.Name.ToLowerInvariant(), pi => pi);

            dynamic dObj = new ExpandoObject();
            var dProps = dObj as IDictionary<string, object>;

            foreach (var prop in props.Keys)
            {
                var val = props[prop].GetValue(instance);
                if (!val.IsNullOrDefault())
                    dProps.Add(prop, val.UnwrapValue());
            }

            await serializationStreamWriter.WriteAsync(dObj);
        }
    }
}