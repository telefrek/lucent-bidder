using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lucent.Common.Serialization.Json
{
    /// <summary>
    /// Json implementation of the ISerializationStreamWriter interface
    /// </summary>
    public class JsonSerializationStreamWriter : ISerializationStreamWriter
    {
        JsonWriter _jsonWriter;
        ISerializationRegistry _registry;
        ILogger _log;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonWriter"></param>
        /// <param name="registry"></param>
        /// <param name="log"></param>
        public JsonSerializationStreamWriter(JsonWriter jsonWriter, ISerializationRegistry registry, ILogger log)
        {
            _jsonWriter = jsonWriter;
            _registry = registry;
            _log = log;
        }

        /// <inheritdoc />
        public void Flush() => _jsonWriter.Flush();

        /// <inheritdoc />
        public async Task FlushAsync() => await _jsonWriter.FlushAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(ExpandoObject value)
        {
            if (value == null)
                _jsonWriter.WriteNull();
            else
            {
                _jsonWriter.WriteStartObject();

                var properties = (IDictionary<string, object>)value;
                if (properties != null)
                {
                    foreach (var prop in properties.Keys)
                    {
                        _jsonWriter.WritePropertyName(prop);
                        var val = properties[prop];
                        if(val == null)
                            continue; // skip nulls

                        var valType = val.GetType();

                        var method = typeof(JsonSerializationStreamWriter).GetMethod("Write", new Type[] { valType });

                        if (method != null)
                            method.Invoke(this, new object[] { val });
                        else
                            Write(val);
                    }
                }

                _jsonWriter.WriteEndObject();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(ExpandoObject[] value)
        {
            if (value == null)
                _jsonWriter.WriteNull();
            else
            {
                _jsonWriter.WriteStartArray();
                foreach (var obj in value)
                    Write(obj);
                _jsonWriter.WriteEndArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public void Write<T>(T value) where T : new()
        {
            if (value.IsNullOrDefault())
                _jsonWriter.WriteNull();
            else
            {
                var serializer = _registry.GetSerializer<T>();
                serializer.Write(this, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public void Write<T>(T[] value) where T : new()
        {
            if (value == null)
                _jsonWriter.WriteNull();
            else
            {
                _jsonWriter.WriteStartArray();
                foreach (var obj in value)
                    Write(obj);
                _jsonWriter.WriteEndArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(bool value) => _jsonWriter.WriteValue(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(double value) => _jsonWriter.WriteValue(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(float value) => _jsonWriter.WriteValue(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(int value) => _jsonWriter.WriteValue(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(uint value) => _jsonWriter.WriteValue(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(long value) => _jsonWriter.WriteValue(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(ulong value) => _jsonWriter.WriteValue(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(string value)
        {
            if (value == null)
                _jsonWriter.WriteNull();
            else
                _jsonWriter.WriteValue(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(string[] value)
        {
            if (value == null)
                _jsonWriter.WriteNull();
            else
            {
                _jsonWriter.WriteStartArray();
                foreach (var obj in value)
                    Write(obj);
                _jsonWriter.WriteEndArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(DateTime value)
        {
            if (value == null)
                _jsonWriter.WriteNull();
            else
            {
                _jsonWriter.WriteValue(value.ToUniversalTime().ToString("o"));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(Guid value)
        {
            if (value == null)
                _jsonWriter.WriteNull();
            else
            {
                _jsonWriter.WriteValue(value.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteAsync(ExpandoObject value)
        {
            if (value == null)
                await _jsonWriter.WriteNullAsync();
            else
            {
                await _jsonWriter.WriteStartObjectAsync();

                var properties = (IDictionary<string, object>)value;
                if (properties != null)
                {
                    foreach (var prop in properties.Keys)
                    {
                        await _jsonWriter.WritePropertyNameAsync(prop);
                        var val = properties[prop];
                        if(val == null) continue;
                        var method = typeof(JsonSerializationStreamWriter).GetMethod("WriteAsync", new Type[] { val.GetType() });
                        if (method != null)
                            await (Task)method.Invoke(this, new object[] { val });
                        else
                            await WriteAsync(val);
                    }
                }

                await _jsonWriter.WriteEndObjectAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteAsync(ExpandoObject[] value)
        {
            if (value == null)
                await _jsonWriter.WriteNullAsync();
            else
            {
                await _jsonWriter.WriteStartArrayAsync();
                foreach (var obj in value)
                    await WriteAsync(obj);
                await _jsonWriter.WriteEndArrayAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task WriteAsync<T>(T value) where T : new()
        {
            if (value.IsNullOrDefault())
                await _jsonWriter.WriteNullAsync();
            else
            {
                var serializer = _registry.GetSerializer<T>();
                await serializer.WriteAsync(this, value, CancellationToken.None);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task WriteAsync<T>(T[] value) where T : new()
        {
            if (value == null)
                await _jsonWriter.WriteNullAsync();
            else
            {
                await _jsonWriter.WriteStartArrayAsync();
                foreach (var obj in value)
                    await WriteAsync(obj);
                await _jsonWriter.WriteEndArrayAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteAsync(bool value) => await _jsonWriter.WriteValueAsync(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteAsync(double value) => await _jsonWriter.WriteValueAsync(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteAsync(float value) => await _jsonWriter.WriteValueAsync(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteAsync(int value) => await _jsonWriter.WriteValueAsync(value);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteAsync(uint value) => await _jsonWriter.WriteValueAsync(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteAsync(long value) => await _jsonWriter.WriteValueAsync(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteAsync(ulong value) => await _jsonWriter.WriteValueAsync(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteAsync(string value)
        {
            if (value == null)
                await _jsonWriter.WriteNullAsync();
            else
                await _jsonWriter.WriteValueAsync(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteAsync(string[] value)
        {
            if (value == null)
                await _jsonWriter.WriteNullAsync();
            else
            {
                await _jsonWriter.WriteStartArrayAsync();
                foreach (var obj in value)
                    await WriteAsync(obj);
                await _jsonWriter.WriteEndArrayAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public async Task WriteAsync(DateTime value)
        {
            if (value == null)
                await _jsonWriter.WriteNullAsync();
            else
            {
                await _jsonWriter.WriteValueAsync(value.ToUniversalTime().ToString("o"));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public async Task WriteAsync(Guid value)
        {
            if (value == null)
                await _jsonWriter.WriteNullAsync();
            else
            {
                await _jsonWriter.WriteValueAsync(value.ToString());
            }
        }

        #region IDisposable
        bool _disposed = false;

        /// <summary>
        /// Disposes of the resources depending on the flag and internal state
        /// </summary>
        /// <param name="disposing">If the caller is requesting resources to be disposed</param>
        protected virtual void _Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _jsonWriter.Flush();
                    _jsonWriter.Close();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Default destructor
        /// </summary>
        ~JsonSerializationStreamWriter()
        {
            _Dispose(false);
        }

        /// <summary>
        /// Public diposable method for the interface
        /// </summary>
        void IDisposable.Dispose()
        {
            _Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        /// <inheritdoc />
        public void Write<T>(PropertyId id, T value) where T : new()
        {
            if(value.IsNullOrDefault())
                return;

            _jsonWriter.WritePropertyName(id.Name);
            Write(value);
        }

        /// <inheritdoc />
        public void Write<T>(PropertyId id, T[] value) where T : new()
        {
            if(value.IsNullOrDefault())
                return;
                
            _jsonWriter.WritePropertyName(id.Name);
            Write(value);
        }

        /// <inheritdoc />
        public void Write(PropertyId id, bool value)
        {
            _jsonWriter.WritePropertyName(id.Name);
            Write(value);
        }

        /// <inheritdoc />
        public void Write(PropertyId id, double value)
        {
            _jsonWriter.WritePropertyName(id.Name);
            Write(value);
        }

        /// <inheritdoc />
        public void Write(PropertyId id, float value)
        {
            _jsonWriter.WritePropertyName(id.Name);
            Write(value);
        }

        /// <inheritdoc />
        public void Write(PropertyId id, int value)
        {
            _jsonWriter.WritePropertyName(id.Name);
            Write(value);
        }

        /// <inheritdoc />
        public void Write(PropertyId id, uint value)
        {
            _jsonWriter.WritePropertyName(id.Name);
            Write(value);
        }

        /// <inheritdoc />
        public void Write(PropertyId id, long value)
        {
            _jsonWriter.WritePropertyName(id.Name);
            Write(value);
        }

        /// <inheritdoc />
        public void Write(PropertyId id, ulong value)
        {
            _jsonWriter.WritePropertyName(id.Name);
            Write(value);
        }

        /// <inheritdoc />
        public void Write(PropertyId id, string value)
        {
            if(value.IsNullOrDefault())
                return;
                
            _jsonWriter.WritePropertyName(id.Name);
            Write(value);
        }

        /// <inheritdoc />
        public void Write(PropertyId id, string[] value)
        {
            if(value.IsNullOrDefault())
                return;
                
            _jsonWriter.WritePropertyName(id.Name);
            Write(value);
        }

        /// <inheritdoc />
        public void Write(PropertyId id, DateTime value)
        {
            if(value.IsNullOrDefault())
                return;
                
            _jsonWriter.WritePropertyName(id.Name);
            Write(value);
        }

        /// <inheritdoc />
        public void Write(PropertyId id, Guid value)
        {
            if(value.IsNullOrDefault())
                return;
                
            _jsonWriter.WritePropertyName(id.Name);
            Write(value);
        }

        /// <inheritdoc />
        public async Task WriteAsync<T>(PropertyId id, T value) where T : new()
        {
            if(value.IsNullOrDefault())
                return;
                
            await _jsonWriter.WritePropertyNameAsync(id.Name);
            await WriteAsync(value);
        }

        /// <inheritdoc />
        public async Task WriteAsync<T>(PropertyId id, T[] value) where T : new()
        {
            if(value.IsNullOrDefault())
                return;
                
            await _jsonWriter.WritePropertyNameAsync(id.Name);
            await WriteAsync(value);
        }

        /// <inheritdoc />
        public async Task WriteAsync(PropertyId id, bool value)
        {
            await _jsonWriter.WritePropertyNameAsync(id.Name);
            await WriteAsync(value);
        }

        /// <inheritdoc />
        public async Task WriteAsync(PropertyId id, double value)
        {
            await _jsonWriter.WritePropertyNameAsync(id.Name);
            await WriteAsync(value);
        }

        /// <inheritdoc />
        public async Task WriteAsync(PropertyId id, float value)
        {
            await _jsonWriter.WritePropertyNameAsync(id.Name);
            await WriteAsync(value);
        }

        /// <inheritdoc />
        public async Task WriteAsync(PropertyId id, int value)
        {
            await _jsonWriter.WritePropertyNameAsync(id.Name);
            await WriteAsync(value);
        }

        /// <inheritdoc />
        public async Task WriteAsync(PropertyId id, uint value)
        {
            await _jsonWriter.WritePropertyNameAsync(id.Name);
            await WriteAsync(value);
        }

        /// <inheritdoc />
        public async Task WriteAsync(PropertyId id, long value)
        {
            await _jsonWriter.WritePropertyNameAsync(id.Name);
            await WriteAsync(value);
        }

        /// <inheritdoc />
        public async Task WriteAsync(PropertyId id, ulong value)
        {
            await _jsonWriter.WritePropertyNameAsync(id.Name);
            await WriteAsync(value);
        }

        /// <inheritdoc />
        public async Task WriteAsync(PropertyId id, string value)
        {
            if(value.IsNullOrDefault())
                return;
                
            await _jsonWriter.WritePropertyNameAsync(id.Name);
            await WriteAsync(value);
        }

        /// <inheritdoc />
        public async Task WriteAsync(PropertyId id, string[] value)
        {
            if(value.IsNullOrDefault())
                return;
                
            await _jsonWriter.WritePropertyNameAsync(id.Name);
            await WriteAsync(value);
        }

        /// <inheritdoc />
        public async Task WriteAsync(PropertyId id, DateTime value)
        {
            if(value.IsNullOrDefault())
                return;
                
            await _jsonWriter.WritePropertyNameAsync(id.Name);
            await WriteAsync(value);
        }

        /// <inheritdoc />
        public async Task WriteAsync(PropertyId id, Guid value)
        {
            if(value.IsNullOrDefault())
                return;
                
            await _jsonWriter.WritePropertyNameAsync(id.Name);
            await WriteAsync(value);
        }

        /// <inheritdoc />
        public async Task StartObjectAsync() => await _jsonWriter.WriteStartObjectAsync();

        /// <inheritdoc />
        public async Task EndObjectAsync() => await _jsonWriter.WriteEndObjectAsync();

        /// <inheritdoc />
        public async Task StartArrayAsync() => await _jsonWriter.WriteStartArrayAsync();

        /// <inheritdoc />
        public async Task EndArrayAsync() => await _jsonWriter.WriteEndArrayAsync();

        /// <inheritdoc />
        public void StartObject() => _jsonWriter.WriteStartObject();

        /// <inheritdoc />
        public void EndObject() => _jsonWriter.WriteEndObject();

        /// <inheritdoc />
        public void StartArray() => _jsonWriter.WriteStartArray();

        /// <inheritdoc />
        public void EndArray() => _jsonWriter.WriteEndArray();
    }
}