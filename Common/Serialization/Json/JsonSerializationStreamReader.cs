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
        readonly ILogger<JsonSerializationStreamReader> _log;
        readonly ISerializationRegistry _registry;

        volatile SerializationToken _token;


        /// <summary>
        /// Default constructor for the stream reader
        /// </summary>
        /// <param name="jsonReader">The JsonReader pointing to the current resource</param>
        /// <param name="registry">The serialization registry to use</param>
        /// <param name="log">The logger to use</param>
        public JsonSerializationStreamReader(JsonReader jsonReader, ISerializationRegistry registry, ILogger<JsonSerializationStreamReader> log)
        {
            _jsonReader = jsonReader;
            _token = SerializationToken.Unknown;
            _registry = registry;
            _log = log;
        }

        /// <summary>
        /// 
        /// </summary>
        public SerializationToken Token => _token;

        /// <summary>
        /// 
        /// </summary>
        public PropertyId Id => new PropertyId { Name = _jsonReader.Value as string };

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        async Task updateTokenAsync()
        {
            // reset the token
            switch (_jsonReader.TokenType)
            {
                case JsonToken.Boolean:
                case JsonToken.Float:
                case JsonToken.String:
                case JsonToken.Integer:
                case JsonToken.Bytes:
                case JsonToken.Date:
                    _token = SerializationToken.Value;
                    break;
                case JsonToken.StartObject:
                    _token = SerializationToken.Object;
                    break;
                case JsonToken.StartArray:
                    _token = SerializationToken.Array;
                    break;
                case JsonToken.PropertyName:
                    _token = SerializationToken.Property;
                    break;
                case JsonToken.EndArray:
                case JsonToken.EndObject:
                    if (await _jsonReader.ReadAsync())
                        await updateTokenAsync();
                    else
                        _token = SerializationToken.EndOfStream;
                    break;
                default:
                    _token = SerializationToken.Unknown;
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void updateToken()
        {
            // reset the token
            switch (_jsonReader.TokenType)
            {
                case JsonToken.Boolean:
                case JsonToken.Float:
                case JsonToken.String:
                case JsonToken.Integer:
                case JsonToken.Bytes:
                case JsonToken.Date:
                    _token = SerializationToken.Value;
                    break;
                case JsonToken.StartObject:
                    _token = SerializationToken.Object;
                    break;
                case JsonToken.StartArray:
                    _token = SerializationToken.Array;
                    break;
                case JsonToken.PropertyName:
                    _token = SerializationToken.Property;
                    break;
                case JsonToken.EndArray:
                case JsonToken.EndObject:
                    if (_jsonReader.Read())
                        updateToken();
                    else
                        _token = SerializationToken.EndOfStream;
                    break;
                default:
                    _token = SerializationToken.Unknown;
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool HasNext()
        {
            if (_jsonReader.Read())
            {
                updateToken();
                return true;
            }

            _token = SerializationToken.EndOfStream;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> HasNextAsync()
        {
            if (await _jsonReader.ReadAsync())
            {
                updateToken();
                return true;
            }

            return false;
        }

        /// <summary>
        ///
        /// </summary>
        public void Skip()
        {
            try
            {
                _jsonReader.Skip();
            }
            finally
            {
                updateToken();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task SkipAsync()
        {
            try
            {
                await _jsonReader.SkipAsync();
            }
            finally
            {
                await updateTokenAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ReadAs<T>() where T : new()
        {
            try
            {
                return _registry.GetSerializer<T>().Read(this);
            }
            finally
            {
                updateToken();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> ReadAsAsync<T>() where T : new()
        {
            try
            {
                return await _registry.GetSerializer<T>().ReadAsync(this, CancellationToken.None);
            }
            finally
            {
                await updateTokenAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] ReadAsArray<T>() where T : new()
        {
            try
            {
                // Get the serializer and a place to store the values
                var serializer = _registry.GetSerializer<T>();
                var array = new List<T>();

                // Check if we are at the start of an array
                if (_jsonReader.TokenType == JsonToken.StartArray)
                    _jsonReader.Read();

                do
                {
                    // Only deserialize started objects
                    if (_jsonReader.TokenType == JsonToken.StartObject)
                        array.Add(serializer.Read(this));

                    // Get out of the loop
                    else if (_jsonReader.TokenType == JsonToken.EndArray)
                        break;

                    // Might even want to toss an exception here...
                    else
                        _jsonReader.Skip();

                } while (_jsonReader.Read());

                return array.ToArray();
            }
            finally
            {
                updateToken();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T[]> ReadAsArrayAsync<T>() where T : new()
        {
            try
            {
                // Get the serializer and a place to store the values
                var serializer = _registry.GetSerializer<T>();
                var array = new List<T>();

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

                } while (await _jsonReader.ReadAsync());

                return array.ToArray();
            }
            finally
            {
                await updateTokenAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ReadBoolean()
        {
            try
            {
                return _jsonReader.ReadAsBoolean().GetValueOrDefault();
            }
            finally
            {
                updateToken();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ReadBooleanAsync()
        {
            try
            {
                return (await _jsonReader.ReadAsBooleanAsync()).GetValueOrDefault();
            }
            finally
            {
                await updateTokenAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double ReadDouble()
        {
            try
            {
                return _jsonReader.ReadAsDouble().GetValueOrDefault();
            }
            finally
            {
                updateToken();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<double> ReadDoubleAsync()
        {
            try
            {
                return (await _jsonReader.ReadAsDoubleAsync()).GetValueOrDefault();
            }
            finally
            {
                await updateTokenAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int ReadInt()
        {
            try
            {
                return _jsonReader.ReadAsInt32().GetValueOrDefault();
            }
            finally
            {
                updateToken();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<int> ReadIntAsync()
        {
            try
            {
                return (await _jsonReader.ReadAsInt32Async()).GetValueOrDefault();
            }
            finally
            {
                await updateTokenAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public long ReadLong()
        {
            try
            {
                return _jsonReader.ReadAsInt32().GetValueOrDefault();
            }
            finally
            {
                updateToken();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<long> ReadLongAsync()
        {
            try
            {
                return (await _jsonReader.ReadAsInt32Async()).GetValueOrDefault();
            }
            finally
            {
                await updateTokenAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public float ReadSingle()
        {
            try
            {
                return (float)_jsonReader.ReadAsDouble().GetValueOrDefault();
            }
            finally
            {
                updateToken();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<float> ReadSingleAsync()
        {
            try
            {
                return (float)(await _jsonReader.ReadAsDoubleAsync()).GetValueOrDefault();
            }
            finally
            {
                await updateTokenAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ReadString()
        {
            try
            {
                return _jsonReader.ReadAsString();
            }
            finally
            {
                updateToken();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<string> ReadStringAsync()
        {
            try
            {
                return await _jsonReader.ReadAsStringAsync();
            }
            finally
            {
                await updateTokenAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DateTime ReadDateTime()
        {
            try
            {
                return DateTime.Parse(_jsonReader.ReadAsString());
            }
            finally
            {
                updateToken();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<DateTime> ReadDateTimeAsync()
        {
            try
            {
                return DateTime.Parse(await _jsonReader.ReadAsStringAsync());
            }
            finally
            {
                await updateTokenAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Guid ReadGuid()
        {
            try
            {
                return Guid.Parse(_jsonReader.ReadAsString());
            }
            finally
            {
                updateToken();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<Guid> ReadGuidAsync()
        {
            try
            {
                return Guid.Parse(await _jsonReader.ReadAsStringAsync());
            }
            finally
            {
                updateToken();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string[] ReadStringArray()
        {
            try
            {
                // Create the temp storage for the array
                var array = new List<string>();

                // Check if we are at the start of an array
                if (_jsonReader.TokenType == JsonToken.StartArray)
                    _jsonReader.Read();

                do
                {
                    // Only deserialize string objects
                    if (_jsonReader.TokenType == JsonToken.String)
                        array.Add(ReadString());

                    // Get out of the loop
                    else if (_jsonReader.TokenType == JsonToken.EndArray)
                        break;

                    // Might even want to toss an exception here...
                    else
                        _jsonReader.Skip();

                } while (_jsonReader.Read());

                return array.ToArray();
            }
            finally
            {
                updateToken();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<string[]> ReadStringArrayAsync()
        {
            try
            {
                // Create the temp storage for the array
                var array = new List<string>();

                // Check if we are at the start of an array
                if (_jsonReader.TokenType == JsonToken.StartArray)
                    await _jsonReader.ReadAsync();

                do
                {
                    // Only deserialize string objects
                    if (_jsonReader.TokenType == JsonToken.String)
                        array.Add(await ReadStringAsync());

                    // Get out of the loop
                    else if (_jsonReader.TokenType == JsonToken.EndArray)
                        break;

                    // Might even want to toss an exception here...
                    else
                        await _jsonReader.SkipAsync();

                } while (await _jsonReader.ReadAsync());

                return array.ToArray();
            }
            finally
            {
                await updateTokenAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public uint ReadUInt()
        {
            try
            {
                return (uint)_jsonReader.ReadAsInt32().GetValueOrDefault();
            }
            finally
            {
                updateToken();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<uint> ReadUIntAsync()
        {
            try
            {
                return (uint)(await _jsonReader.ReadAsInt32Async()).GetValueOrDefault();
            }
            finally
            {
                await updateTokenAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ulong ReadULong()
        {
            try
            {
                return (ulong)_jsonReader.ReadAsInt32().GetValueOrDefault();
            }
            finally
            {
                updateToken();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<ulong> ReadULongAsync()
        {
            try
            {
                return (ulong)(await _jsonReader.ReadAsInt32Async()).GetValueOrDefault();
            }
            finally
            {
                await updateTokenAsync();
            }
        }

        public bool HasMoreProperties()
        {
            if (_jsonReader.TokenType == JsonToken.StartObject)
            {
                if (!HasNext())
                    return false;
                return _jsonReader.TokenType == JsonToken.PropertyName;
            }

            if (_jsonReader.TokenType == JsonToken.EndObject)
                return false;

            if (_jsonReader.TokenType != JsonToken.PropertyName)
                _jsonReader.Read();

            return _jsonReader.TokenType == JsonToken.PropertyName;
        }

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