using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Lucent.Common.Protobuf
{
    /// <summary>
    /// Reads a stream of bytes and translates into a protobuf object.
    /// </summary>
    public class ProtobufReader : IDisposable
    {
        /// <summary>
        /// Internal member that allows acccess to the raw stream
        /// </summary>
        protected Stream _raw;
        bool _leaveOpen;

        volatile WireType _fieldType = WireType.UNKNOWN;
        ulong _fieldNumber = ulong.MaxValue;

        /// <summary>
        /// Gets the current field wire type
        /// </summary>
        /// <returns>The current field WireType</returns>
        public WireType FieldType { get { return _fieldType; } }

        /// <summary>
        /// Gets the current field id
        /// </summary>
        /// <returns>The current field number</returns>
        public ulong FieldNumber { get { return _fieldNumber; } }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="raw">The stream to read from</param>
        /// <param name="leaveOpen"></param>
        public ProtobufReader(Stream raw, bool leaveOpen = false)
        {
            _leaveOpen = leaveOpen;

            if (raw is BufferedStream)
                _raw = raw;
            else
                _raw = new BufferedStream(raw);
        }

        /// <summary>
        /// Skip the next value
        /// </summary>
        public void Skip()
        {
            switch (_fieldType)
            {
                case WireType.START_GROUP:
                case WireType.END_GROUP:
                    throw new InvalidOperationException("Grouping is not supported");
                case WireType.FIXED_32:
                    if (_raw.CanSeek)
                        _raw.Seek(4, SeekOrigin.Current);
                    else
                        _raw.Position += 4;
                    break;
                case WireType.FIXED_64:
                    if (_raw.CanSeek)
                        _raw.Seek(8, SeekOrigin.Current);
                    else
                        _raw.Position += 8;
                    break;
                case WireType.LEN_ENCODED:
                    // Skip the next n blocks
                    var len = ReadInt64();
                    if (_raw.CanSeek)
                        _raw.Seek(len, SeekOrigin.Current);
                    else
                        _raw.Position += len;
                    break;
                default:
                    ReadVarint();
                    break;
            }
        }

        /// <summary>
        /// Check if there is more data on the stream
        /// </summary>
        /// <returns>False if the stream is empty</returns>
        public virtual bool Read()
        {
            // End of stream?
            if (_raw.Position == _raw.Length)
                return false;

            // Read the packed field information
            var v = ReadVarint();
            _fieldType = (WireType)(v & 0x7);
            _fieldNumber = (v >> 3);

            return true;
        }

        /// <summary>
        /// Validates if a stream is empty
        /// </summary>
        /// <returns>True if the position is at or over the length</returns>
        public virtual bool IsEmpty() => _raw.Position >= _raw.Length;

        /// <summary>
        /// Read a boolean (1/0) byte off the stream.
        /// </summary>
        /// <returns>True if the byte value is 1</returns>
        public bool ReadBool() => ReadVarint() == 1;

        /// <summary>
        /// Read a fixed 32 bit field from the stream
        /// </summary>
        /// <returns>An unsigned integer (4 bytes)</returns>
        public virtual uint ReadFixed32()
        {
            var u = 0U;
            for (var i = 0; i < 4; ++i)
                u |= (uint)_raw.ReadByte() << (i << 3);

            return u;
        }

        /// <summary>
        /// Read a fixed 64 bit field from the stream
        /// </summary>
        /// <returns>An unsigned long (8 bytes)</returns>
        public virtual ulong ReadFixed64()
        {
            var u = 0UL;
            for (var i = 0; i < 8; ++i)
                u |= (ulong)_raw.ReadByte() << (i << 3);

            return u;
        }

        /// <summary>
        /// Reads a float value (fixed32) from the stream
        /// </summary>
        /// <returns>A floating point value</returns>
        public float ReadFloat() => BitConverter.Int32BitsToSingle((int)ReadFixed32());

        /// <summary>
        /// Reads a double value (fixed64) from the stream
        /// </summary>
        /// <returns></returns>
        public double ReadDouble() => BitConverter.Int64BitsToDouble((long)ReadFixed64());

        /// <summary>
        /// Read the next encoded value as a string
        /// </summary>
        /// <returns>A UTF-8 string representation of the value</returns>
        public virtual string ReadString()
        {
            var len = ReadInt32();
            var buf = new byte[len];

            // Read the bytes as a UTF8 string
            if (len == _raw.Read(buf, 0, len))
                return Encoding.UTF8.GetString(buf);

            // Return null if the read failed
            return null;
        }

        /// <summary>
        /// Read a single int32 (signed) value from the stream
        /// </summary>
        /// <returns>A signed integer</returns>
        public int ReadInt32() => (int)(ReadVarint() & 0xFFFFFFFF);

        /// <summary>
        /// Read a single uint32 (unsigned) value from the stream
        /// </summary>
        /// <returns>An unsigned integer</returns>
        public uint ReadUInt32() => (uint)(ReadVarint() & 0xFFFFFFFF);

        /// <summary>
        /// Read a single zigzag encoded int32 value from the stream
        /// </summary>
        /// <returns>A signed integer</returns>
        public int ReadSInt32()
        {
            var v = ReadUInt32();
            return (int)((v >> 1) ^ (~(v & 1) + 1));
        }

        /// <summary>
        /// Read a single long (signed) value from the stream
        /// </summary>
        /// <returns>A signed long</returns>
        public long ReadInt64() => (long)ReadVarint();

        /// <summary>
        /// Read a single ulong (unsigned) value from the stream
        /// </summary>
        /// <returns>An unsigned long</returns>
        public ulong ReadUInt64() => ReadVarint();

        /// <summary>
        /// Read a single zigzag encoded long (signed) value from the stream
        /// </summary>
        /// <returns>A signed long</returns>
        public long ReadSInt64()
        {
            var v = ReadVarint();
            return (long)((v >> 1) ^ (~(v & 1) + 1));
        }

        /// <summary>
        /// Utility method to get an object reader for embedded messages
        /// </summary>
        /// <returns></returns>
        public virtual ProtobufReader GetNextMessageReader() => new LenEncodedReader(this._raw, (long)ReadVarint());

        /// <summary>
        /// Read the next variable sized numeric value from the stream
        /// </summary>
        /// <returns>The raw bytes representing the number</returns>
        protected virtual ulong ReadVarint()
        {
            var v = 0UL;

            // Read the next N bytes off the stream
            for (var i = 0; i < 8; ++i)
            {
                var b = (byte)_raw.ReadByte();
                v |= (ulong)(b & 0x7F) << (i * 7);
                if ((b & 0x80) == 0)
                    break;
            }

            return v;
        }

        /// <summary>
        /// Skip the next value
        /// </summary>
        public async Task SkipAsync()
        {
            switch (_fieldType)
            {
                case WireType.START_GROUP:
                case WireType.END_GROUP:
                    throw new InvalidOperationException("Grouping is not supported");
                case WireType.FIXED_32:
                    if (_raw.CanSeek)
                        _raw.Seek(4, SeekOrigin.Current);
                    else
                        _raw.Position += 4;
                    break;
                case WireType.FIXED_64:
                    if (_raw.CanSeek)
                        _raw.Seek(8, SeekOrigin.Current);
                    else
                        _raw.Position += 8;
                    break;
                case WireType.LEN_ENCODED:
                    // Skip the next n blocks
                    var len = await ReadInt64Async();
                    if (_raw.CanSeek)
                        _raw.Seek(len, SeekOrigin.Current);
                    else
                        _raw.Position += len;
                    break;
                default:
                    await ReadVarintAsync();
                    break;
            }
        }

        /// <summary>
        /// Check if there is more data on the stream
        /// </summary>
        /// <returns>False if the stream is empty</returns>
        public async virtual Task<bool> ReadAsync()
        {
            // End of stream?
            if (_raw.Position == _raw.Length)
                return false;

            // Read the packed field information
            var v = await ReadVarintAsync();
            _fieldType = (WireType)(v & 0x7);
            _fieldNumber = (v >> 3);

            return true;
        }

        /// <summary>
        /// Read a boolean (1/0) byte off the stream.
        /// </summary>
        /// <returns>True if the byte value is 1</returns>
        public async Task<bool> ReadBoolAsync() => await ReadVarintAsync() == 1;

        /// <summary>
        /// Read a fixed 32 bit field from the stream
        /// </summary>
        /// <returns>An unsigned integer (4 bytes)</returns>
        public virtual Task<uint> ReadFixed32Async()
        {
            var u = 0U;
            for (var i = 0; i < 4; ++i)
                u |= (uint)_raw.ReadByte() << (i << 3);

            return Task.FromResult(u);
        }

        /// <summary>
        /// Read a fixed 64 bit field from the stream
        /// </summary>
        /// <returns>An unsigned long (8 bytes)</returns>
        public virtual Task<ulong> ReadFixed64Async()
        {
            var u = 0UL;
            for (var i = 0; i < 8; ++i)
                u |= (ulong)_raw.ReadByte() << (i << 3);

            return Task.FromResult(u);
        }

        /// <summary>
        /// Reads a float value (fixed32) from the stream
        /// </summary>
        /// <returns>A floating point value</returns>
        public async Task<float> ReadFloatAsync() => BitConverter.Int32BitsToSingle((int)await ReadFixed32Async());

        /// <summary>
        /// Reads a double value (fixed64) from the stream
        /// </summary>
        /// <returns></returns>
        public async Task<double> ReadDoubleAsync() => BitConverter.Int64BitsToDouble((long)await ReadFixed64Async());

        /// <summary>
        /// Read the next encoded value as a string
        /// </summary>
        /// <returns>A UTF-8 string representation of the value</returns>
        public virtual async Task<string> ReadStringAsync()
        {
            var len = await ReadInt32Async();
            var buf = new byte[len];

            // Read the bytes as a UTF8 string
            if (len == await _raw.ReadAsync(buf, 0, len))
                return Encoding.UTF8.GetString(buf);

            // Return null if the read failed
            return null;
        }

        /// <summary>
        /// Read a single int32 (signed) value from the stream
        /// </summary>
        /// <returns>A signed integer</returns>
        public async Task<int> ReadInt32Async() => (int)(await ReadVarintAsync() & 0xFFFFFFFF);

        /// <summary>
        /// Read a single uint32 (unsigned) value from the stream
        /// </summary>
        /// <returns>An unsigned integer</returns>
        public async Task<uint> ReadUInt32Async() => (uint)(await ReadVarintAsync() & 0xFFFFFFFF);

        /// <summary>
        /// Read a single zigzag encoded int32 value from the stream
        /// </summary>
        /// <returns>A signed integer</returns>
        public async Task<int> ReadSInt32Async()
        {
            var v = await ReadUInt32Async();
            return (int)((v >> 1) ^ (~(v & 1) + 1));
        }

        /// <summary>
        /// Read a single long (signed) value from the stream
        /// </summary>
        /// <returns>A signed long</returns>
        public async Task<long> ReadInt64Async() => (long)await ReadVarintAsync();

        /// <summary>
        /// Read a single ulong (unsigned) value from the stream
        /// </summary>
        /// <returns>An unsigned long</returns>
        public async Task<ulong> ReadUInt64Async() => await ReadVarintAsync();

        /// <summary>
        /// Read a single zigzag encoded long (signed) value from the stream
        /// </summary>
        /// <returns>A signed long</returns>
        public async Task<long> ReadSInt64Async()
        {
            var v = await ReadVarintAsync();
            return (long)((v >> 1) ^ (~(v & 1) + 1));
        }

        /// <summary>
        /// Utility method to get an object reader for embedded messages
        /// </summary>
        /// <returns></returns>
        public virtual async Task<ProtobufReader> GetNextMessageReaderAsync() => new LenEncodedReader(this._raw, (long)await ReadVarintAsync());

        /// <summary>
        /// Read the next variable sized numeric value from the stream
        /// </summary>
        /// <returns>The raw bytes representing the number</returns>
        protected virtual Task<ulong> ReadVarintAsync()
        {
            var v = 0UL;

            // Read the next N bytes off the stream
            for (var i = 0; i < 8; ++i)
            {
                var b = (byte)_raw.ReadByte();
                v |= (ulong)(b & 0x7F) << (i * 7);
                if ((b & 0x80) == 0)
                    break;
            }

            return Task.FromResult(v);
        }

        /// <summary>
        /// Closes the reader
        /// </summary>
        public void Close()
        {
            if(!_leaveOpen)
                _raw.Close();
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
                    _raw.Flush();
                    Close();
                }

                _raw = null;
                _disposed = true;
            }
        }

        /// <summary>
        /// Default destructor
        /// </summary>
        ~ProtobufReader()
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


    /// <summary>
    /// Class for reading only part of a stream without closing it.
    /// 
    /// This is especially useful for reading embedded messages.
    /// </summary>
    sealed class LenEncodedReader : ProtobufReader
    {
        long _rem;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="s"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public LenEncodedReader(Stream s, long length) : base(s, true) => _rem = length;

        /// <summary>
        /// Check if there is more data on the stream
        /// </summary>
        /// <returns>False if the stream is empty</returns>
        public sealed override bool Read()
        {
            if (_rem > 0)
                return base.Read();

            return false;
        }

        /// <summary>
        /// Check if there is more data on the stream
        /// </summary>
        /// <returns>False if the stream is empty</returns>
        public sealed override async Task<bool> ReadAsync()
        {
            if (_rem > 0)
                return await base.ReadAsync();

            return false;
        }

        /// <summary>
        /// Utility method to get an object reader for embedded messages
        /// </summary>
        /// <returns></returns>
        public override ProtobufReader GetNextMessageReader()
        {
            var reader = new LenEncodedReader(this._raw, (long)ReadVarint());
            _rem -= reader._rem;
            return reader;
        }

        /// <summary>
        /// Utility method to get an object reader for embedded messages
        /// </summary>
        /// <returns></returns>
        public override async Task<ProtobufReader> GetNextMessageReaderAsync()
        {
            var reader = new LenEncodedReader(this._raw, (long)await ReadVarintAsync());
            _rem -= reader._rem;
            return reader;
        }

        public sealed override bool IsEmpty() => _rem <= 0;

        /// <summary>
        /// Read the next encoded value as a string
        /// </summary>
        /// <returns>A UTF-8 string representation of the value</returns>
        public sealed override string ReadString()
        {
            var start = _raw.Position;
            var s = base.ReadString();
            _rem -= _raw.Position - start;

            return s;
        }

        /// <summary>
        /// Read the next encoded value as a string
        /// </summary>
        /// <returns>A UTF-8 string representation of the value</returns>
        public sealed override async Task<string> ReadStringAsync()
        {
            var start = _raw.Position;
            var s = await base.ReadStringAsync();
            _rem -= _raw.Position - start;

            return s;
        }

        /// <summary>
        /// Read a fixed 32 bit field from the stream
        /// </summary>
        /// <returns>An unsigned integer (4 bytes)</returns>
        public sealed override uint ReadFixed32()
        {
            _rem -= 4;
            return base.ReadFixed32();
        }

        /// <summary>
        /// Read a fixed 32 bit field from the stream
        /// </summary>
        /// <returns>An unsigned integer (4 bytes)</returns>
        public sealed override async Task<uint> ReadFixed32Async()
        {
            _rem -= 4;
            return await base.ReadFixed32Async();
        }

        /// <summary>
        /// Read a fixed 64 bit field from the stream
        /// </summary>
        /// <returns>An unsigned long (8 bytes)</returns>
        public sealed override ulong ReadFixed64()
        {
            _rem -= 8;
            return base.ReadFixed64();
        }

        /// <summary>
        /// Read a fixed 64 bit field from the stream
        /// </summary>
        /// <returns>An unsigned long (8 bytes)</returns>
        public sealed override async Task<ulong> ReadFixed64Async()
        {
            _rem -= 8;
            return await base.ReadFixed64Async();
        }

        /// <summary>
        /// Read the next variable sized numeric value from the stream
        /// </summary>
        /// <returns>The raw bytes representing the number</returns>
        protected sealed override ulong ReadVarint()
        {
            var v = 0UL;

            // Read the next N bytes off the stream
            for (var i = 0; i < 8; ++i)
            {
                _rem--;
                var b = (byte)_raw.ReadByte();
                v |= (ulong)(b & 0x7F) << (i * 7);
                if ((b & 0x80) == 0)
                    break;
            }

            return v;
        }

        /// <summary>
        /// Read the next variable sized numeric value from the stream
        /// </summary>
        /// <returns>The raw bytes representing the number</returns>
        protected sealed override Task<ulong> ReadVarintAsync()
        {
            var v = 0UL;

            // Read the next N bytes off the stream
            for (var i = 0; i < 8; ++i)
            {
                _rem--;
                var b = (byte)_raw.ReadByte();
                v |= (ulong)(b & 0x7F) << (i * 7);
                if ((b & 0x80) == 0)
                    break;
            }

            return Task.FromResult(v);
        }

        protected sealed override void _Dispose(bool disposing)
        {
            // Never dispose of resources upstream
            base._Dispose(false);
        }
    }
}