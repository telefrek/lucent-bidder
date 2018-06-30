using System.Threading;
using System.Threading.Tasks;

namespace Lucent.Common.Serialization
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// This class abstracts the serialization format away from individual consumers to allow
    /// objects to be easily transferred between multiple serialization formats, depending on
    /// the use case
    /// </remarks>
    /// <typeparam name="T">A type that implements the <see>Lucent.Common.Serialization.ILucentSerializable</see> interface</typeparam>
    public interface IEntitySerializer<T>
    {

        /// <summary>
        ///  Reads the object contents from the stream
        /// </summary>
        /// <param name="serializationStreamReader">The ISerializationStreamReader to read from</param>
        /// <returns>An object from the stream</returns>
        T Read(ISerializationStreamReader serializationStreamReader);

        /// <summary>
        /// Reads the object contents from the stream asynchronously
        /// </summary>
        /// <param name="serializationStreamReader">The ISerializationStreamReader to read from</param>
        /// <param name="token">A cancellation token used to stop/abandon reading</param>
        /// <returns>An async Task that returns an object from the stream</returns>
        Task<T> ReadAsync(ISerializationStreamReader serializationStreamReader, CancellationToken token);

        /// <summary>
        /// Writes the object contents to the stream
        /// </summary>
        /// <param name="serializationStreamWriter">The ISerializationStreamWriter to write to</param>
        /// <param name="instance">The instance to serialize</param>
        void Write(ISerializationStreamWriter serializationStreamWriter, T instance);
        
        /// <summary>
        /// Writes the object asynchronously to the stream
        /// </summary>
        /// <param name="serializationStreamWriter">The ISerializationStreamWriter to write to</param>
        /// <param name="instance">The instance to serialize</param>
        /// <param name="token">A cancellation token used to stop/abandon writing</param>
        /// <returns>A Task indicating when the serialization is completed</returns>
        Task WriteAsync(ISerializationStreamWriter serializationStreamWriter, T instance, CancellationToken token);
    }
}