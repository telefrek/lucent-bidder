using System.IO;
using System.IO.Compression;
using System.Text;
using Lucent.Common.Protobuf;
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
        public ISerializationStream CreateStream(SerializationFormat format)
            => new SerializationStream(new MemoryStream(), format, this);

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
    }
}