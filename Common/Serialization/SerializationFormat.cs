namespace Lucent.Common.Serialization
{
    /// <summary>
    /// Identifies the format for the serialized object
    /// </summary>
    public enum SerializationFormat
    {
        /// <summary>
        /// No serialization, just cast the object
        /// </summary>
        NONE = 0x0,
        /// <summary>
        /// Serialized as a JSON object
        /// </summary>
        JSON = 0x1,
        /// <summary>
        /// Serialized as a Protobuf object (binary)
        /// </summary>
        PROTOBUF = 0x2,
        /// <summary>
        /// Serialization stream is compressed
        /// </summary>
        COMPRESSED = 0x4
    }
}