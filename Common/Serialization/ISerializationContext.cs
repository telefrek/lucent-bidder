using System.IO;
using System.Threading.Tasks;

namespace Lucent.Common.Serialization
{
    /// <summary>
    /// Represents a serialization context instance for managing serialization objects.
    /// </summary>
    public interface ISerializationContext
    {
        /// <summary>
        /// Write the object to the stream
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="instance"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task Write<T>(ILucentObjectWriter writer, T instance) where T : new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="property"></param>
        /// <param name="instance"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task WriteObject<T>(ILucentWriter writer, PropertyId property, T instance) where T : new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="instance"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task WriteArrayObject<T>(ILucentArrayWriter writer, T instance) where T : new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="property"></param>
        /// <param name="instances"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task WriteArray<T>(ILucentWriter writer, PropertyId property, T[] instances);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="target"></param>
        /// <param name="leaveOpen"></param>
        /// <param name="format"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task WriteTo<T>(T instance, Stream target, bool leaveOpen, SerializationFormat format) where T : new();

        /// <summary>
        /// Read the next object from the stream
        /// </summary>
        /// <param name="reader"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> Read<T>(ILucentObjectReader reader) where T : new();

        /// <summary>
        /// Read the next item as an object
        /// </summary>
        /// <param name="reader"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> ReadObject<T>(ILucentReader reader) where T : new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> ReadArrayObject<T>(ILucentArrayReader reader) where T : new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T[]> ReadArray<T>(ILucentReader reader);

        /// <summary>
        /// Read the object from the stream
        /// </summary>
        /// <param name="target"></param>
        /// <param name="leaveOpen"></param>
        /// <param name="format"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> ReadFrom<T>(Stream target, bool leaveOpen, SerializationFormat format) where T : new();
    }
}