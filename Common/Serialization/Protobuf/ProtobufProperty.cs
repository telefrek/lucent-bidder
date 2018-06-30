using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Protobuf;

namespace Lucent.Common.Serialization.Protobuf
{
    /// <summary>
    /// Protobuf property information used for protobuf serialization
    /// </summary>
    public sealed class ProtobufProperty
    {
        /// <summary>
        /// Gets/Sets the property index
        /// </summary>
        public ulong PropertyIndex { get; set; }

        /// <summary>
        /// Gets/Sets the wire type
        /// </summary>
        public WireType Type { get; set; }
    }
}