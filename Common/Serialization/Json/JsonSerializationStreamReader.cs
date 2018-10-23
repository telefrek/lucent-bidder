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
    /// Implementation of the ISerializationStreamReader for Json format
    /// </summary>
    public class JsonSerializationStreamReader : ISerializationStreamReader
    {
        readonly JsonReader _jsonReader;
        readonly ILogger _log;
        readonly ISerializationRegistry _registry;


        /// <summary>
        /// Default constructor for the stream reader
        /// </summary>
        /// <param name="jsonReader">The JsonReader pointing to the current resource</param>
        /// <param name="registry">The serialization registry to use</param>
        /// <param name="log">The logger to use</param>
        public JsonSerializationStreamReader(JsonReader jsonReader, ISerializationRegistry registry, ILogger log)
        {
            _jsonReader = jsonReader;
            _registry = registry;
            _log = log;
        }

        /// <summary>
        /// 
        /// </summary>
        public PropertyId Id => new PropertyId { Name = _jsonReader.Value as string };

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool HasNext() => HasNextAsync().Result;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> HasNextAsync() => await _jsonReader.ReadAsync();

        /// <summary>
        ///
        /// </summary>
        public void Skip() => SkipAsync().Wait();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task SkipAsync() => await _jsonReader.SkipAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ReadAs<T>() where T : new() => ReadAsAsync<T>().Result;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> ReadAsAsync<T>() where T : new()
            => await _registry.GetSerializer<T>().ReadAsync(this, CancellationToken.None);


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] ReadAsArray<T>() where T : new() => ReadAsArrayAsync<T>().Result;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T[]> ReadAsArrayAsync<T>() where T : new()
        {
            // Get the serializer and a place to store the values
            var serializer = _registry.GetSerializer<T>();
            var array = new List<T>();

            // Advance if still on the property
            if (_jsonReader.TokenType == JsonToken.PropertyName)
                await _jsonReader.ReadAsync();

            // Check if we are at the start of an array
            if (_jsonReader.TokenType == JsonToken.StartArray)
                await _jsonReader.ReadAsync();

            do
            {
                // Only deserialize started objects
                if (_jsonReader.TokenType == JsonToken.StartObject)
                    array.Add(await serializer.ReadAsync(this, CancellationToken.None));

                // Get out of the loop
                else if (_jsonReader.TokenType == JsonToken.EndArray)
                    break;

                // Might even want to toss an exception here...
                else
                    await _jsonReader.SkipAsync();

            } while (_jsonReader.TokenType != JsonToken.EndArray && await _jsonReader.ReadAsync());

            return array.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ReadBoolean() => ReadBooleanAsync().Result;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ReadBooleanAsync() => (await _jsonReader.ReadAsBooleanAsync()).GetValueOrDefault();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double ReadDouble() => ReadDoubleAsync().Result;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<double> ReadDoubleAsync() => (await _jsonReader.ReadAsDoubleAsync()).GetValueOrDefault();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int ReadInt() => ReadIntAsync().Result;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<int> ReadIntAsync() => (await _jsonReader.ReadAsInt32Async()).GetValueOrDefault();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public long ReadLong() => ReadLongAsync().Result;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<long> ReadLongAsync() => (await _jsonReader.ReadAsInt32Async()).GetValueOrDefault();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public float ReadSingle() => ReadSingleAsync().Result;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<float> ReadSingleAsync() => (float)(await _jsonReader.ReadAsDoubleAsync()).GetValueOrDefault();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ReadString() => ReadStringAsync().Result;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<string> ReadStringAsync() => await _jsonReader.ReadAsStringAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DateTime ReadDateTime() => ReadDateTimeAsync().Result;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<DateTime> ReadDateTimeAsync() => DateTime.Parse(await _jsonReader.ReadAsStringAsync());

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Guid ReadGuid() => ReadGuidAsync().Result;


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<Guid> ReadGuidAsync() => Guid.Parse(await _jsonReader.ReadAsStringAsync());

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string[] ReadStringArray() => ReadStringArrayAsync().Result;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<string[]> ReadStringArrayAsync()
        {
            // Create the temp storage for the array
            var array = new List<string>();

            if (_jsonReader.TokenType == JsonToken.PropertyName)
                await _jsonReader.ReadAsync();

            // Check if we are at the start of an array
            if (_jsonReader.TokenType == JsonToken.StartArray)
                await _jsonReader.ReadAsync();

            do
            {
                // Only deserialize string objects
                if (_jsonReader.TokenType == JsonToken.String)
                    array.Add(_jsonReader.Value as string);

                // Get out of the loop
                else if (_jsonReader.TokenType == JsonToken.EndArray)
                    break;

                // Might even want to toss an exception here...
                else
                    await _jsonReader.SkipAsync();

            } while (await _jsonReader.ReadAsync());

            return array.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public uint ReadUInt() => ReadUIntAsync().Result;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<uint> ReadUIntAsync() => (uint)(await _jsonReader.ReadAsInt32Async()).GetValueOrDefault();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ulong ReadULong() => ReadULongAsync().Result;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<ulong> ReadULongAsync() => (ulong)(await _jsonReader.ReadAsInt32Async()).GetValueOrDefault();

        /// <inheritdoc />
        public bool HasMoreProperties() => HasMorePropertiesAsync().Result;

        /// <inheritdoc />
        public async Task<bool> HasMorePropertiesAsync()
        {
            if (_jsonReader.TokenType == JsonToken.StartObject)
            {
                if (!await HasNextAsync())
                    return false;
                return _jsonReader.TokenType == JsonToken.PropertyName;
            }

            if (_jsonReader.TokenType == JsonToken.EndObject)
                return false;

            if (_jsonReader.TokenType != JsonToken.PropertyName)
                await _jsonReader.ReadAsync();

            return _jsonReader.TokenType == JsonToken.PropertyName;
        }

        /// <inheritdoc />
        public bool StartObject() => StartObjectAsync().Result;

        /// <inheritdoc />
        public async Task<bool> StartObjectAsync()
        {
            if (_jsonReader.TokenType == JsonToken.PropertyName)
                return await HasNextAsync() && _jsonReader.TokenType == JsonToken.StartObject; ;

            return _jsonReader.TokenType == JsonToken.StartObject;
        }

        /// <inheritdoc />
        public bool EndObject() => EndObjectAsync().Result;

        /// <inheritdoc />
        public async Task<bool> EndObjectAsync()
        {
            if (_jsonReader.TokenType == JsonToken.EndObject)
            {
                await _jsonReader.ReadAsync();
                return true;
            }

            return false;
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
                    _jsonReader.Close();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Default destructor
        /// </summary>
        ~JsonSerializationStreamReader()
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