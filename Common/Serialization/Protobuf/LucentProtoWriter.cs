using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Lucent.Common.Protobuf;

namespace Lucent.Common.Serialization.Protobuf
{
    /// <summary>
    /// Protobuf writer
    /// </summary>
    public class LucentProtoWriter : ILucentWriter
    {
        readonly ProtobufWriter protobufWriter;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="target"></param>
        /// <param name="leaveOpen"></param>
        public LucentProtoWriter(Stream target, bool leaveOpen = false) => protobufWriter = new ProtobufWriter(target, leaveOpen);

        /// <inheritdoc/>
        public SerializationFormat Format { get { return SerializationFormat.PROTOBUF; } }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, int value)
        {
            if (!value.IsNullOrDefault())
            {
                await protobufWriter.WriteFieldAsync(property.Id, WireType.VARINT);
                await protobufWriter.WriteAsync(value);
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, uint value)
        {
            if (!value.IsNullOrDefault())
            {
                await protobufWriter.WriteFieldAsync(property.Id, WireType.VARINT);
                await protobufWriter.WriteAsync(value);
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, long value)
        {
            if (!value.IsNullOrDefault())
            {
                await protobufWriter.WriteFieldAsync(property.Id, WireType.VARINT);
                await protobufWriter.WriteAsync(value);
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, ulong value)
        {
            if (!value.IsNullOrDefault())
            {
                await protobufWriter.WriteFieldAsync(property.Id, WireType.VARINT);
                await protobufWriter.WriteAsync(value);
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, string value)
        {
            if (!value.IsNullOrDefault())
            {
                await protobufWriter.WriteFieldAsync(property.Id, WireType.LEN_ENCODED);
                await protobufWriter.WriteAsync(value);
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, double value)
        {
            if (!value.IsNullOrDefault())
            {
                await protobufWriter.WriteFieldAsync(property.Id, WireType.FIXED_64);
                await protobufWriter.WriteAsync(value);
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, float value)
        {
            if (!value.IsNullOrDefault())
            {
                await protobufWriter.WriteFieldAsync(property.Id, WireType.FIXED_32);
                await protobufWriter.WriteAsync(value);
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, Guid value)
        {
            if (!value.IsNullOrDefault())
            {
                await protobufWriter.WriteFieldAsync(property.Id, WireType.LEN_ENCODED);
                await protobufWriter.WriteAsync(value.EncodeGuid());
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, DateTime value)
        {
            if (!value.IsNullOrDefault())
            {
                await protobufWriter.WriteFieldAsync(property.Id, WireType.VARINT);
                await protobufWriter.WriteAsync(value.ToFileTimeUtc());
            }
        }

        /// <inheritdoc/>
        public async Task<ILucentObjectWriter> CreateObjectWriter(PropertyId property)
        {
            await protobufWriter.WriteFieldAsync(property.Id, WireType.VARINT);
            return new LucentProtoObjectWriter(protobufWriter);
        }

        /// <inheritdoc/>
        public async Task<ILucentArrayWriter> CreateArrayWriter(PropertyId property)
        {
            await protobufWriter.WriteFieldAsync(property.Id, WireType.VARINT);
            return new LucentProtoArrayWriter(protobufWriter);
        }

        /// <inheritdoc/>
        public void Dispose() => protobufWriter.Close();

        /// <inheritdoc/>
        public async Task Flush() => await protobufWriter.FlushAsync();
    }
}