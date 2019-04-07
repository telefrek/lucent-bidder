using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Lucent.Common.Protobuf;

namespace Lucent.Common.Serialization.Protobuf
{
    /// <summary>
    /// Protobuf Array Writer
    /// </summary>
    public class LucentProtoArrayWriter : ILucentArrayWriter
    {
        readonly ProtobufWriter protoWriter;
        readonly ProtobufWriter original;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="writer">The current writer</param>
        public LucentProtoArrayWriter(ProtobufWriter writer)
        {
            original = writer;
            protoWriter = new ProtobufWriter(new MemoryStream());
        }

        /// <inheritdoc/>
        public async Task WriteAsync(bool value) => await protoWriter.WriteAsync(value);

        /// <inheritdoc/>
        public async Task WriteAsync(int value) => await protoWriter.WriteAsync(value);

        /// <inheritdoc/>
        public async Task WriteAsync(uint value) => await protoWriter.WriteAsync(value);

        /// <inheritdoc/>
        public async Task WriteAsync(long value) => await protoWriter.WriteAsync(value);

        /// <inheritdoc/>
        public async Task WriteAsync(ulong value) => await protoWriter.WriteAsync(value);

        /// <inheritdoc/>
        public async Task WriteAsync(string value) => await protoWriter.WriteAsync(value);

        /// <inheritdoc/>
        public async Task WriteAsync(double value) => await protoWriter.WriteAsync(value);

        /// <inheritdoc/>
        public async Task WriteAsync(float value) => await protoWriter.WriteAsync(value);

        /// <inheritdoc/>
        public async Task WriteAsync(Guid value) => await protoWriter.WriteAsync(value.EncodeGuid());

        /// <inheritdoc/>
        public async Task WriteAsync(DateTime value) => await protoWriter.WriteAsync(value.ToFileTimeUtc());

        /// <inheritdoc/>
        public async Task WriteEnd() => await original.CopyFromAsync(protoWriter);

        /// <inheritdoc/>
        public Task<ILucentObjectWriter> CreateObjectWriter() => 
            Task.FromResult((ILucentObjectWriter)new LucentProtoObjectWriter(protoWriter));

        /// <inheritdoc/>
        public Task<ILucentArrayWriter> CreateArrayWriter() => Task.FromResult((ILucentArrayWriter)new LucentProtoArrayWriter(protoWriter));

        /// <inheritdoc/>
        public void Dispose() => protoWriter.Close();

        /// <inheritdoc/>
        public async Task Flush() => await protoWriter.FlushAsync();
    }    
}