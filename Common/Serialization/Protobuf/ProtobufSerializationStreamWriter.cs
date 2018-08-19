using System;
using System.Dynamic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Protobuf;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Serialization.Protobuf
{
    /// <summary>
    /// Implementation of the ISerializationStreamWriter that uses protobuf
    /// </summary>
    public class ProtobufSerializationStreamWriter : ISerializationStreamWriter
    {
        ILogger<ProtobufSerializationStreamWriter> _log;
        ISerializationRegistry _registry;
        ProtobufWriter _protoWriter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="protoWriter"></param>
        /// <param name="registry"></param>
        /// <param name="log"></param>
        public ProtobufSerializationStreamWriter(ProtobufWriter protoWriter, ISerializationRegistry registry, ILogger<ProtobufSerializationStreamWriter> log)
        {
            _protoWriter = protoWriter;
            _registry = registry;
            _log = log;
        }
        
        public void Flush() => _protoWriter.Flush();

        public async Task FlushAsync() => await _protoWriter.FlushAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(ExpandoObject value) => throw new SerializationException("Cannot format protobuf object without mapping");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(ExpandoObject[] value) => throw new SerializationException("Cannot format protobuf object without mapping");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public void Write<T>(T value) where T : new()
        {
            if (typeof(T).IsAssignableFrom(typeof(ProtobufProperty)))
            {
                var prop = value as ProtobufProperty;
                _protoWriter.WriteField(prop.PropertyIndex, prop.Type);
            }
            else if (value == null)
                throw new SerializationException("Cannot serialize a null object");
            else
            {
                _registry.Guard<T>();
                var serializer = _registry.GetSerializer<T>();
                using (var ms = new MemoryStream())
                {
                    using (var protoTemp = new ProtobufWriter(ms))
                    {
                        var tempWriter = new ProtobufSerializationStreamWriter(protoTemp, _registry, _log);
                        serializer.Write(tempWriter, value);
                        protoTemp.Flush();

                        // Copy the length encoding
                        _protoWriter.CopyFrom(protoTemp);
                    }
                }
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
                throw new SerializationException("Cannot serialize a null object");
            else
            {
                using (var ms = new MemoryStream())
                {
                    using (var protoTemp = new ProtobufWriter(ms))
                    {
                        var tempWriter = new ProtobufSerializationStreamWriter(protoTemp, _registry, _log);
                        foreach (var obj in value)
                            tempWriter.Write(obj);
                        protoTemp.Flush();

                        // Copy the length encoding
                        _protoWriter.CopyFrom(protoTemp);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(bool value) => _protoWriter.Write(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(double value) => _protoWriter.Write(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(float value) => _protoWriter.Write(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(int value) => _protoWriter.Write(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(uint value) => _protoWriter.Write(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(long value) => _protoWriter.Write(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(ulong value) => _protoWriter.Write(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(string value) => _protoWriter.Write(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Write(string[] value)
        {
            using (var ms = new MemoryStream())
            {
                using (var protoTemp = new ProtobufWriter(ms))
                {
                    var tempWriter = new ProtobufSerializationStreamWriter(protoTemp, _registry, _log);
                    foreach (var obj in value)
                        tempWriter.Write(obj);
                    protoTemp.Flush();

                    // Copy the length encoding
                    _protoWriter.CopyFrom(protoTemp);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task WriteAsync(ExpandoObject value) => throw new SerializationException("Cannot format protobuf object without mapping");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task WriteAsync(ExpandoObject[] value) => throw new SerializationException("Cannot format protobuf object without mapping");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task WriteAsync<T>(T value) where T : new()
        {
            if (typeof(T).IsAssignableFrom(typeof(ProtobufProperty)))
            {
                var prop = value as ProtobufProperty;
                await _protoWriter.WriteFieldAsync(prop.PropertyIndex, prop.Type);
            }
            else if (value == null)
                throw new SerializationException("Cannot serialize a null object");
            else
            {
                _registry.Guard<T>();
                var serializer = _registry.GetSerializer<T>();
                using (var ms = new MemoryStream())
                {
                    using (var protoTemp = new ProtobufWriter(ms))
                    {
                        var tempWriter = new ProtobufSerializationStreamWriter(protoTemp, _registry, _log);
                        await serializer.WriteAsync(tempWriter, value, CancellationToken.None);
                        await protoTemp.FlushAsync();

                        // Copy the length encoding
                        await _protoWriter.CopyFromAsync(protoTemp);
                    }
                }
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
                throw new SerializationException("Cannot serialize a null object");
            else
            {
                using (var ms = new MemoryStream())
                {
                    using (var protoTemp = new ProtobufWriter(ms))
                    {
                        var tempWriter = new ProtobufSerializationStreamWriter(protoTemp, _registry, _log);
                        foreach (var obj in value)
                            await tempWriter.WriteAsync(obj);
                        await protoTemp.FlushAsync();

                        // Copy the length encoding
                        await _protoWriter.CopyFromAsync(protoTemp);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteAsync(bool value) => await _protoWriter.WriteAsync(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteAsync(double value) => await _protoWriter.WriteAsync(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteAsync(float value) => await _protoWriter.WriteAsync(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteAsync(int value) => await _protoWriter.WriteAsync(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteAsync(uint value) => await _protoWriter.WriteAsync(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteAsync(long value) => await _protoWriter.WriteAsync(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteAsync(ulong value) => await _protoWriter.WriteAsync(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteAsync(string value) => await _protoWriter.WriteAsync(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteAsync(string[] value)
        {
            using (var ms = new MemoryStream())
            {
                using (var protoTemp = new ProtobufWriter(ms))
                {
                    var tempWriter = new ProtobufSerializationStreamWriter(protoTemp, _registry, _log);
                    foreach (var obj in value)
                        await tempWriter.WriteAsync(obj);
                    await protoTemp.FlushAsync();

                    // Copy the length encoding
                    await _protoWriter.CopyFromAsync(protoTemp);
                }
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
                    _protoWriter.Flush();
                    _protoWriter.Close();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Default destructor
        /// </summary>
        ~ProtobufSerializationStreamWriter()
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