using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Lucent.Common.Protobuf;
using Lucent.Common.Serialization._Internal;
using Lucent.Common.Serialization.Json;
using Lucent.Common.Serialization.Protobuf;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lucent.Common.Serialization
{
    /// <summary>
    /// Default implementation for serialization contexts
    /// </summary>
    public class LucentSerializationContext : ISerializationContext
    {
        readonly ISerializationRegistry _registry;
        readonly ILogger _log;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="registry">The serialization registry to use</param>
        /// <param name="logger">A logger for the environment</param>
        public LucentSerializationContext(ISerializationRegistry registry, ILogger<LucentSerializationContext> logger)
        {
            _registry = registry;
            _log = logger;
        }

        /// <inheritdoc />
        public ISerializationStreamReader CreateReader(Stream target, bool leaveOpen, SerializationFormat format)
        {
            if (format.HasFlag(SerializationFormat.COMPRESSED))
                target = new GZipStream(target, CompressionMode.Compress, leaveOpen);

            if (format.HasFlag(SerializationFormat.PROTOBUF))
                return new ProtobufSerializationStreamReader(new ProtobufReader(target, leaveOpen), _registry, _log);
            else
                return new JsonSerializationStreamReader(new JsonTextReader(new StreamReader(target)) { CloseInput = !leaveOpen }, _registry, _log);
        }

        /// <inheritdoc />
        public ISerializationStreamWriter CreateWriter(Stream target, bool leaveOpen, SerializationFormat format)
        {
            if (format.HasFlag(SerializationFormat.COMPRESSED))
                target = new GZipStream(target, CompressionMode.Decompress, leaveOpen);

            if (format.HasFlag(SerializationFormat.PROTOBUF))
                return new ProtobufSerializationStreamWriter(new ProtobufWriter(target, leaveOpen), _registry, _log);
            else
                return new JsonSerializationStreamWriter(new JsonTextWriter(new StreamWriter(target, Encoding.UTF8, 4096, leaveOpen)), _registry, _log);
        }

        /// <inheritdoc />
        public ISerializationStream WrapStream(Stream target, bool leaveOpen, SerializationFormat format)
            => new SerializationStream(target, format, this, leaveOpen);


        /// <inheritdoc />
        public async Task Write<T>(ILucentObjectWriter writer, T instance) where T : new() => await writer.Write(this, instance);

        /// <inheritdoc />
        public async Task WriteObject<T>(ILucentWriter writer, PropertyId property, T instance) where T : new()
        {
            await Write<T>(await writer.CreateObjectWriter(property), instance);
            await writer.Flush();
        }

        /// <inheritdoc />
        public async Task<T> Read<T>(ILucentObjectReader reader) where T : new() => await reader.Read<T>(this);

        /// <inheritdoc/>
        public async Task<T> ReadObject<T>(ILucentReader reader) where T : new() => await Read<T>(await reader.GetObjectReader());

        /// <inheritdoc />
        public async Task<T> ReadFrom<T>(Stream target, bool leaveOpen, SerializationFormat format)
            where T : new()
        {
            if (format.HasFlag(SerializationFormat.COMPRESSED))
                target = new GZipStream(target, CompressionMode.Decompress);

            switch (format)
            {
                case SerializationFormat.PROTOBUF:
                    return await ReadObject<T>(new LucentProtoReader(target, leaveOpen));
                default:
                    return await ReadObject<T>(new LucentJsonReader(target, leaveOpen));
            }
        }

        /// <inheritdoc />
        public async Task WriteTo<T>(T instance, Stream target, bool leaveOpen, SerializationFormat format)
            where T : new()
        {
            if (format.HasFlag(SerializationFormat.COMPRESSED))
                target = new GZipStream(target, CompressionMode.Compress);

            switch (format)
            {
                case SerializationFormat.PROTOBUF:
                    await Write<T>(await new LucentProtoWriter(target, leaveOpen).AsObjectWriter(), instance);
                    break;
                default:
                    await Write<T>(await new LucentJsonWriter(target, leaveOpen).AsObjectWriter(), instance);
                    break;
            }
        }

    }
}