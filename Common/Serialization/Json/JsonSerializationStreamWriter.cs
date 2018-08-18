using System;
using System.Collections.Generic;
using System.Dynamic;
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
        ILogger<JsonSerializationStreamWriter> _log;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonWriter"></param>
        /// <param name="registry"></param>
        /// <param name="log"></param>
        public JsonSerializationStreamWriter(JsonWriter jsonWriter, ISerializationRegistry registry, ILogger<JsonSerializationStreamWriter> log)
        {
            _jsonWriter = jsonWriter;
            _registry = registry;
            _log = log;
        }

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
                        var method = typeof(JsonSerializationStreamWriter).GetMethod("Write", new Type[] { val.GetType() });
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
            if (value == null)
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
            if (value == null)
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
    }
}