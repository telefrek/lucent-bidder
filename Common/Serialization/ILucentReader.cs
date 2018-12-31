using System;
using System.Threading.Tasks;

namespace Lucent.Common.Serialization
{
    /// <summary>
    /// Reader
    /// </summary>
    public interface ILucentReader : IDisposable
    {
        /// <summary>
        /// Gets the current reader format
        /// </summary>
        /// <value></value>
        SerializationFormat Format { get; }

        /// <summary>
        /// Read the next property asynchronously
        /// </summary>
        /// <returns></returns>
        Task<PropertyId> NextAsync();

        /// <summary>
        /// Read the next value as a boolean asynchronously
        /// </summary>
        /// <returns></returns>
        Task<bool> NextBoolean();

        /// <summary>
        /// Read the next value as an int asynchronously
        /// </summary>
        /// <returns></returns>
        Task<int> NextInt();

        /// <summary>
        /// Read the next value as an unsigned int asynchronously
        /// </summary>
        /// <returns></returns>
        Task<uint> NextUInt();

        /// <summary>
        /// Read the next value as a long asynchronously
        /// </summary>
        /// <returns></returns>
        Task<long> NextLong();

        /// <summary>
        /// Read the next value as an unsigned long asynchronously
        /// </summary>
        /// <returns></returns>
        Task<ulong> NextULong();

        /// <summary>
        /// Read the next value as a double asynchronously
        /// </summary>
        /// <returns></returns>
        Task<double> NextDouble();

        /// <summary>
        /// Read the next value as a single asynchronously
        /// </summary>
        /// <returns></returns>
        Task<float> NextSingle();

        /// <summary>
        /// Read the next value as a string asynchronously
        /// </summary>
        /// <returns></returns>
        Task<string> NextString();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<Guid> NextGuid();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<DateTime> NextDateTime();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        Task<TEnum> NextEnum<TEnum>();

        /// <summary>
        /// Read the next object as a collection of bytes
        /// </summary>
        /// <returns></returns>
        Task<byte[]> NextObjBytes();

        /// <summary>
        /// Skip the next item in the reader asynchronously
        /// </summary>
        /// <returns></returns>
        Task Skip();

        /// <summary>
        /// Get an object reader
        /// </summary>
        /// <returns></returns>
        Task<ILucentObjectReader> GetObjectReader();

        /// <summary>
        /// A an array reader
        /// </summary>
        /// <returns></returns>
        Task<ILucentArrayReader> GetArrayReader();
    }

    /// <summary>
    /// Reads an object from the reader
    /// </summary>
    public interface ILucentObjectReader : ILucentReader
    {
        /// <summary>
        /// Check to see if the object has more properties
        /// </summary>
        /// <returns></returns>
        Task<bool> IsComplete();
    }

    /// <summary>
    /// Reads an array from the reader
    /// </summary>
    public interface ILucentArrayReader : ILucentReader
    {
        /// <summary>
        /// Check if the array is complete
        /// </summary>
        /// <returns></returns>
        Task<bool> IsComplete();
    }
}