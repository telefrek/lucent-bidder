using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Lucent.Common.Protobuf
{
    /// <summary>
    /// Writes a stream of bytes using the protobuf protocol
    /// </summary>
    public class ProtobufWriter : IDisposable
    {
        Stream _raw;
        bool _leaveOpen;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="raw">The stream to write to</param>
        /// <param name="leaveOpen">Flag to indicate if the stream should be left open</param>
        public ProtobufWriter(Stream raw, bool leaveOpen = false)
        {
            _leaveOpen = leaveOpen;

            // Buffer, this would be ugly otherwise
            if (raw is BufferedStream)
                _raw = raw;
            else
                _raw = new BufferedStream(raw);
        }

        /// <summary>
        /// Flush the underlying streams
        /// </summary>
        public void Flush() => _raw.Flush();

        public async Task FlushAsync() => await _raw.FlushAsync();

        /// <summary>
        /// Closes the underlying resources
        /// </summary>
        public void Close()
        {
            if(!_leaveOpen)
                _raw.Close();
        }

        /// <summary>
        /// Write the field encoding information.
        /// 
        /// Note: this can be done in any order, but sequential has better performance for some readers.
        /// </summary>
        /// <param name="fieldNumber">The field number</param>
        /// <param name="type">The type of value</param>
        public void WriteField(ulong fieldNumber, WireType type) => WriteVarint((fieldNumber << 3) | (ulong)type);

        /// <summary>
        /// Copy the contents from one writer to another
        /// </summary>
        /// <param name="src">The source writer to copy from</param>
        public void CopyFrom(ProtobufWriter src)
        {
            WriteVarint((ulong)src._raw.Length);

            // This may not be entirely safe, but only used from MemoryStream for now so...meh
            src._raw.Seek(0, SeekOrigin.Begin);
            src._raw.CopyTo(_raw);
        }

        /// <summary>
        /// Write a fixed 32 bit value to the stream.
        /// </summary>
        /// <param name="value">The value to encode (4 bytes)</param>
        public void WriteFixed32(uint value)
        {
            for (var i = 0; i < 4; ++i)
                _raw.WriteByte((byte)((value >> (i << 3)) & 0xFF));
        }

        /// <summary>
        /// Write a float value to the stream.
        /// </summary>
        /// <param name="value">The float value to write.</param>
        public void Write(float value) => WriteFixed32((uint)BitConverter.SingleToInt32Bits(value));

        /// <summary>
        /// Write a fixed 64 bit value to the stream.
        /// </summary>
        /// <param name="value">The value to encode (8 bytes)</param>
        public void WriteFixed64(ulong value)
        {
            for (var i = 0; i < 8; ++i)
                _raw.WriteByte((byte)((value >> (i << 3)) & 0xFF));
        }

        /// <summary>
        /// Write a double value to the stream.
        /// </summary>
        /// <param name="value">The double value to write</param>
        public void Write(double value) => WriteFixed64((ulong)BitConverter.DoubleToInt64Bits(value));

        /// <summary>
        /// Write a signed (zigzag) value to the stream.
        /// </summary>
        /// <param name="value">The signed int to write</param>
        public void WriteSInt32(int value) => WriteVarint((ulong)((value << 1) ^ (value >> 31)));

        /// <summary>
        /// Write a signed (zigzag) value to the stream.
        /// </summary>
        /// <param name="value">The signed long to write</param>
        public void WriteSInt64(long value) => WriteVarint((ulong)((value << 1) ^ (value >> 63)));

        /// <summary>
        /// Write a boolean value as a (1/0) byte to the stream.
        /// </summary>
        /// <param name="value">The value to write</param>
        public void Write(bool value) => WriteVarint(value ? 1UL : 0UL);

        /// <summary>
        /// Write the given string as a UTF-8 encoded value.
        /// </summary>
        /// <param name="value">The string to write</param>
        public void Write(string value)
        {
            WriteVarint((ulong)value.Length);
            _raw.Write(Encoding.UTF8.GetBytes(value), 0, Encoding.UTF8.GetByteCount(value));
        }

        /// <summary>
        /// Writes an integer to the stream
        /// </summary>
        /// <param name="value">The integer value to write</param>
        public void Write(int value) => WriteVarint((ulong)value);

        /// <summary>
        /// Writes an unsigned integer to the stream
        /// </summary>
        /// <param name="value">The unsigned integer value to write</param>
        public void Write(uint value) => WriteVarint((ulong)value);

        /// <summary>
        /// Writes a long to the stream
        /// </summary>
        /// <param name="value">The long value to write</param>
        public void Write(long value) => WriteVarint((ulong)value);

        /// <summary>
        /// Writes an unsigned long to the stream
        /// </summary>
        /// <param name="value">The unsigned long value to write</param>
        public void Write(ulong value) => WriteVarint(value);

        /// <summary>
        /// Write a variable byte encoded value
        /// </summary>
        /// <param name="val">The value to encode</param>
        private void WriteVarint(ulong val)
        {
            // Write chunks until the value is < 128
            while (val > 0x7F)
            {
                // Write the last 7 bits
                _raw.WriteByte((byte)((val & 0x7F) | 0x80));
                val >>= 7;
            }

            // Write the remaining bits
            _raw.WriteByte((byte)(val & 0x7F));
        }

        /// <summary>
        /// Write the field encoding information.
        /// 
        /// Note: this can be done in any order, but sequential has better performance for some readers.
        /// </summary>
        /// <param name="fieldNumber">The field number</param>
        /// <param name="type">The type of value</param>
        public async Task WriteFieldAsync(ulong fieldNumber, WireType type) => await WriteVarintAsync((fieldNumber << 3) | (ulong)type);

        /// <summary>
        /// Copy the contents from one writer to another
        /// </summary>
        /// <param name="src">The source writer to copy from</param>
        public async Task CopyFromAsync(ProtobufWriter src)
        {
            await WriteFixed64Async((ulong)src._raw.Length);

            // This may not be entirely safe, but only used from MemoryStream for now so...meh
            src._raw.Seek(0, SeekOrigin.Begin);
            await src._raw.CopyToAsync(_raw);
        }

        /// <summary>
        /// Write a fixed 32 bit value to the stream.
        /// </summary>
        /// <param name="value">The value to encode (4 bytes)</param>
        public Task WriteFixed32Async(uint value)
        {
            for (var i = 0; i < 4; ++i)
                _raw.WriteByte((byte)((value >> (i << 3)) & 0xFF));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Write a float value to the stream.
        /// </summary>
        /// <param name="value">The float value to write.</param>
        public async Task WriteAsync(float value) => await WriteFixed32Async((uint)BitConverter.SingleToInt32Bits(value));

        /// <summary>
        /// Write a fixed 64 bit value to the stream.
        /// </summary>
        /// <param name="value">The value to encode (8 bytes)</param>
        public Task WriteFixed64Async(ulong value)
        {
            for (var i = 0; i < 8; ++i)
                _raw.WriteByte((byte)((value >> (i << 3)) & 0xFF));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Write a double value to the stream.
        /// </summary>
        /// <param name="value">The double value to write</param>
        public async Task WriteAsync(double value) => await WriteFixed64Async((ulong)BitConverter.DoubleToInt64Bits(value));

        /// <summary>
        /// Write a signed (zigzag) value to the stream.
        /// </summary>
        /// <param name="value">The signed int to write</param>
        public async Task WriteSInt32Async(int value) => await WriteVarintAsync((ulong)((value << 1) ^ (value >> 31)));

        /// <summary>
        /// Write a signed (zigzag) value to the stream.
        /// </summary>
        /// <param name="value">The signed long to write</param>
        public async Task WriteSInt64Async(long value) => await WriteVarintAsync((ulong)((value << 1) ^ (value >> 63)));

        /// <summary>
        /// Write a boolean value as a (1/0) byte to the stream.
        /// </summary>
        /// <param name="value">The value to write</param>
        public async Task WriteAsync(bool value) => await WriteVarintAsync(value ? 1UL : 0UL);

        /// <summary>
        /// Write the given string as a UTF-8 encoded value.
        /// </summary>
        /// <param name="value">The string to write</param>
        public async Task WriteAsync(string value)
        {
            await WriteVarintAsync((ulong)value.Length);
            await _raw.WriteAsync(Encoding.UTF8.GetBytes(value), 0, Encoding.UTF8.GetByteCount(value));
        }

        /// <summary>
        /// Writes an integer to the stream
        /// </summary>
        /// <param name="value">The integer value to write</param>
        public async Task WriteAsync(int value) => await WriteVarintAsync((ulong)value);

        /// <summary>
        /// Writes an unsigned integer to the stream
        /// </summary>
        /// <param name="value">The unsigned integer value to write</param>
        public async Task WriteAsync(uint value) => await WriteVarintAsync((ulong)value);

        /// <summary>
        /// Writes a long to the stream
        /// </summary>
        /// <param name="value">The long value to write</param>
        public async Task WriteAsync(long value) => await WriteVarintAsync((ulong)value);

        /// <summary>
        /// Writes an unsigned long to the stream
        /// </summary>
        /// <param name="value">The unsigned long value to write</param>
        public async Task WriteAsync(ulong value) => await WriteVarintAsync(value);

        /// <summary>
        /// Write a variable byte encoded value
        /// </summary>
        /// <param name="val">The value to encode</param>
        private Task WriteVarintAsync(ulong val)
        {
            // Write chunks until the value is < 128
            while (val > 0x7F)
            {
                // Write the last 7 bits
                _raw.WriteByte((byte)((val & 0x7F) | 0x80));
                val >>= 7;
            }

            // Write the remaining bits
            _raw.WriteByte((byte)(val & 0x7F));

            return Task.CompletedTask;
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
        ~ProtobufWriter()
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