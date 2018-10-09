using System.IO;

namespace Lucent.Common.Serialization
{
    /// <summary>
    /// Represents a serialization context instance for managing serialization objects.
    /// </summary>
    public interface ISerializationContext
    {
        /// <summary>
        /// Creates a new serialization stream with the given format
        /// </summary>
        /// <param name="format">The desired serialization format</param>
        /// <returns>A new serialization stream</returns>
        ISerializationStream CreateStream(SerializationFormat format);

        /// <summary>
        /// Wraps an existing stream as a serialization stream with the given format
        /// </summary>
        /// <param name="target">The stream to decorate</param>
        /// <param name="leaveOpen">Flag indicating if the stream should be left open when closed</param>
        /// <param name="format">The desired serialization format</param>
        /// <returns>A new serialization stream</returns>
        ISerializationStream WrapStream(Stream target, bool leaveOpen, SerializationFormat format);

        /// <summary>
        /// Creates a serialization reader from the given stream and format
        /// </summary>
        /// <param name="target">The stream to decorate</param>
        /// <param name="leaveOpen">Flag indicating if the stream should be left open when closed</param>
        /// <param name="format">The desired serialization format</param>
        /// <returns>A new serialization stream reader</returns>
        ISerializationStreamReader CreateReader(Stream target, bool leaveOpen, SerializationFormat format);

        /// <summary>
        /// Creates a serialization writer from teh given stream and format
        /// </summary>
        /// <param name="target">The stream to decorate</param>
        /// <param name="leaveOpen">Flag indicating if the stream should be left open when closed</param>
        /// <param name="format">The desired serialization format</param>
        /// <returns>A new serialization stream writer</returns>
        ISerializationStreamWriter CreateWriter(Stream target, bool leaveOpen, SerializationFormat format);
    }
}