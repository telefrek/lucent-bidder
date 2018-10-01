using System.IO;
using System.Text;
using Lucent.Common.Serialization.Json;
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
        public ISerializationStreamReader CreateReader(Stream target, bool leaveOpen, SerializationFormat format) => new JsonSerializationStreamReader(new JsonTextReader(new StreamReader(target)) { CloseInput = !leaveOpen }, _registry, _log);

        /// <inheritdoc />
        public ISerializationStream CreateStream(SerializationFormat format)
            => new SerializationStream(new MemoryStream(), format, this);

        /// <inheritdoc />
        public ISerializationStreamWriter CreateWriter(Stream target, bool leaveOpen, SerializationFormat format) => new JsonSerializationStreamWriter(new JsonTextWriter(new StreamWriter(target, Encoding.UTF8, 4096, leaveOpen)), _registry, _log);

        /// <inheritdoc />
        public ISerializationStream WrapStream(Stream target, bool leaveOpen, SerializationFormat format)
            => new SerializationStream(target, format, this, leaveOpen);
    }
}