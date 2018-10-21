using System;
using System.Threading;
using System.Threading.Tasks;
using Lucent.Common.Serialization;

namespace Lucent.Common.OpenRTB.Serializers
{
    /// <summary>
    /// Default class for serializing OpenRTB enum objects
    /// </summary>
    /// <typeparam name="T">The type of enum to serialize</typeparam>
    public sealed class EnumEntitySerializer<T> : IEntitySerializer<T>
    {
        /// <summary>
        /// Reads the next enum of type T from the stream
        /// </summary>
        /// <param name="serializationStreamReader">The current stream</param>
        /// <returns>The next enum value from the stream</returns>
        public T Read(ISerializationStreamReader serializationStreamReader)
        {
            // Parse the enum
            return (T)Enum.Parse(typeof(T), serializationStreamReader.ReadInt().ToString(), true);
        }

        /// <summary>
        /// Reads the next enum of type T from the stream asynchronously
        /// </summary>
        /// <param name="serializationStreamReader">The current stream</param>
        /// <param name="token"></param>
        /// <returns>The next enum value from the stream</returns>
        public async Task<T> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token)
        {
            // Parse the enum
            return (T)Enum.Parse(typeof(T), (await serializationStreamReader.ReadIntAsync()).ToString(), true);
        }

        /// <summary>
        /// Writes the enum value to the stream
        /// </summary>
        /// <param name="serializationStreamWriter">The stream to write to</param>
        /// <param name="instance">The value to write</param>
        public void Write(ISerializationStreamWriter serializationStreamWriter, T instance) => serializationStreamWriter.Write(Convert.ToInt32(instance));

        /// <summary>
        /// Writes the enum value to the stream asynchronously
        /// </summary>
        /// <param name="serializationStreamWriter">The stream to write to</param>
        /// <param name="instance">The value to write</param>
        /// <param name="token"></param>
        public async Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, T instance, CancellationToken token) => await serializationStreamWriter.WriteAsync(Convert.ToInt32(instance));
    }
}