namespace Lucent.Common.Serialization
{
    /// <summary>
    /// A stream that is used for serialization
    /// </summary>
    public interface ISerializationStream
    {
        /// <summary>
        /// Get the format for the serialization stream
        /// </summary>
        SerializationFormat Format { get; }

        /// <summary>
        /// Gets a reader for the stream
        /// </summary>
        ISerializationStreamReader Reader { get; }

        /// <summary>
        /// Gets a writer for the stream
        /// </summary>
        ISerializationStreamWriter Writer { get; }
    }
}