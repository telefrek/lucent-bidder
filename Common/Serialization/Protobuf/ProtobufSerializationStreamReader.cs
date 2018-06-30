using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Protobuf;
using Microsoft.Extensions.Logging;

namespace Lucent.Common.Serialization.Protobuf
{
    /// <summary>
    /// Implementation of the ISerializationStreamReader using protobuf
    /// </summary>
    public class ProtobufSerializationStreamReader : ISerializationStreamReader
    {
        ProtobufReader _protoReader;
        ISerializationRegistry _registry;
        ILogger<ProtobufSerializationStreamReader> _log;

        volatile SerializationToken _token;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="protoReader"></param>
        /// <param name="registry"></param>
        /// <param name="log"></param>
        public ProtobufSerializationStreamReader(ProtobufReader protoReader, ISerializationRegistry registry, ILogger<ProtobufSerializationStreamReader> log)
        {
            _protoReader = protoReader;
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
        public object Value => (_protoReader.FieldNumber << 3) & ((ulong)_protoReader.FieldType);


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task updateTokenAsync()
        {
            // reset the token
            switch (_protoReader.FieldType)
            {
                // Note this is more complicated for readers since objects
                // have to be known ahead of time
                case WireType.VARINT:
                case WireType.FIXED_32:
                case WireType.FIXED_64:
                    _token = SerializationToken.Value;
                    break;
                case WireType.LEN_ENCODED:
                    _token = SerializationToken.Object;
                    break;
                default:
                    _token = SerializationToken.Unknown;
                    break;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        void updateToken()
        {
            // reset the token
            switch (_protoReader.FieldType)
            {
                // Note this is more complicated for readers since objects
                // have to be known ahead of time
                case WireType.VARINT:
                case WireType.FIXED_32:
                case WireType.FIXED_64:
                    _token = SerializationToken.Value;
                    break;
                case WireType.LEN_ENCODED:
                    _token = SerializationToken.Object;
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
            if (_protoReader.Read())
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
            if (await _protoReader.ReadAsync())
            {
                await updateTokenAsync();
                return true;
            }

            _token = SerializationToken.EndOfStream;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ReadAs<T>()
        {
            try
            {
                // Hate the double casting =\
                if (typeof(ProtobufProperty).IsAssignableFrom(typeof(T)))
                    return (T)(object)new ProtobufProperty { PropertyIndex = _protoReader.FieldNumber, Type = _protoReader.FieldType };

                _token.Guard(SerializationToken.Object);
                _registry.Guard<T>();

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
        public T[] ReadAsArray<T>()
        {
            _token.Guard(SerializationToken.Object);
            _registry.Guard<T>();

            // Get the serializer and a place to store the values
            var serializer = _registry.GetSerializer<T>();
            var array = new List<T>();

            // Get the portion of the reader associated with this chunk
            var arrayReader = _protoReader.GetNextMessageReader();
            var arrayStreamReader = new ProtobufSerializationStreamReader(arrayReader, _registry, _log);

            // Keep reading objects from the stream
            while (arrayStreamReader.HasNext())
                array.Add(arrayStreamReader.ReadAs<T>());

            // Return the array
            return array.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T[]> ReadAsArrayAsync<T>()
        {
            _token.Guard(SerializationToken.Object);
            _registry.Guard<T>();

            // Get the serializer and a place to store the values
            var serializer = _registry.GetSerializer<T>();
            var array = new List<T>();

            // Get the portion of the reader associated with this chunk
            var arrayReader = await _protoReader.GetNextMessageReaderAsync();
            var arrayStreamReader = new ProtobufSerializationStreamReader(arrayReader, _registry, _log);

            // Keep reading objects from the stream
            while (await arrayStreamReader.HasNextAsync())
                array.Add(await arrayStreamReader.ReadAsAsync<T>());

            // Return the array
            return array.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> ReadAsAsync<T>()
        {
            try
            {
                // Hate the double casting =\
                if (typeof(ProtobufProperty).IsAssignableFrom(typeof(T)))
                    return (T)(object)new ProtobufProperty { PropertyIndex = _protoReader.FieldNumber, Type = _protoReader.FieldType };

                _token.Guard(SerializationToken.Object);
                _registry.Guard<T>();

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
        /// <returns></returns>
        public bool ReadBoolean()
        {
            _token.Guard(SerializationToken.Value);
            return _protoReader.ReadBool();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ReadBooleanAsync()
        {
            _token.Guard(SerializationToken.Value);
            return await _protoReader.ReadBoolAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double ReadDouble()
        {
            _token.Guard(SerializationToken.Value);
            return _protoReader.ReadDouble();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<double> ReadDoubleAsync()
        {
            _token.Guard(SerializationToken.Value);
            return await _protoReader.ReadDoubleAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public dynamic ReadDynamic() => throw new SerializationException("Cannot read dynamic objects as protobuf");

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public dynamic[] ReadDynamicArray() => throw new SerializationException("Cannot read dynamic objects as protobuf");

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<dynamic[]> ReadDynamicArrayAsync() => throw new SerializationException("Cannot read dynamic objects as protobuf");

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<dynamic> ReadDynamicAsync() => throw new SerializationException("Cannot read dynamic objects as protobuf");

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int ReadInt()
        {
            _token.Guard(SerializationToken.Value);
            return _protoReader.ReadInt32();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<int> ReadIntAsync()
        {
            _token.Guard(SerializationToken.Value);
            return await _protoReader.ReadInt32Async();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public long ReadLong()
        {
            _token.Guard(SerializationToken.Value);
            return _protoReader.ReadInt64();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<long> ReadLongAsync()
        {
            _token.Guard(SerializationToken.Value);
            return await _protoReader.ReadInt64Async();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public float ReadSingle()
        {
            _token.Guard(SerializationToken.Value);
            return _protoReader.ReadFloat();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<float> ReadSingleAsync()
        {
            _token.Guard(SerializationToken.Value);
            return await _protoReader.ReadFloatAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ReadString()
        {
            _token.Guard(SerializationToken.Object);
            return _protoReader.ReadString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string[] ReadStringArray()
        {
            _token.Guard(SerializationToken.Object);

            // Get the serializer and a place to store the values
            var array = new List<string>();

            // Get the portion of the reader associated with this chunk
            var arrayReader = _protoReader.GetNextMessageReader();
            var arrayStreamReader = new ProtobufSerializationStreamReader(arrayReader, _registry, _log);

            // Keep reading objects from the stream
            while (arrayStreamReader.HasNext())
                array.Add(arrayStreamReader.ReadString());

            // Return the array
            return array.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<string[]> ReadStringArrayAsync()
        {
            _token.Guard(SerializationToken.Object);

            // Get the serializer and a place to store the values
            var array = new List<string>();

            // Get the portion of the reader associated with this chunk
            var arrayReader = await _protoReader.GetNextMessageReaderAsync();
            var arrayStreamReader = new ProtobufSerializationStreamReader(arrayReader, _registry, _log);

            // Keep reading objects from the stream
            while (await arrayStreamReader.HasNextAsync())
                array.Add(await arrayStreamReader.ReadStringAsync());

            // Return the array
            return array.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<string> ReadStringAsync()
        {
            _token.Guard(SerializationToken.Object);
            return await _protoReader.ReadStringAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public uint ReadUInt()
        {
            _token.Guard(SerializationToken.Value);
            return _protoReader.ReadUInt32();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<uint> ReadUIntAsync()
        {
            _token.Guard(SerializationToken.Value);
            return await _protoReader.ReadUInt32Async();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ulong ReadULong()
        {
            _token.Guard(SerializationToken.Value);
            return _protoReader.ReadUInt64();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<ulong> ReadULongAsync()
        {
            _token.Guard(SerializationToken.Value);
            return await _protoReader.ReadUInt64Async();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Skip()
        {
            _protoReader.Skip();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task SkipAsync()
        {
            await _protoReader.SkipAsync();
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
                    _protoReader.Close();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Default destructor
        /// </summary>
        ~ProtobufSerializationStreamReader()
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