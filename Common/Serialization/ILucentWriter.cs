using System;
using System.Threading.Tasks;

namespace Lucent.Common.Serialization
{
    /// <summary>
    /// Writes to a stream
    /// </summary>
    public interface ILucentWriter : IDisposable
    {
        /// <summary>
        /// Gets the current writer format
        /// </summary>
        /// <value></value>
        SerializationFormat Format { get; }

        /// <summary>
        /// Writes the property and value to the underlying stream with the appropriate format
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task WriteAsync(PropertyId property, int value);

        /// <summary>
        /// Writes the property and value to the underlying stream with the appropriate format
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns></returns>        Task WriteAsync(PropertyId property, uint value);
        Task WriteAsync(PropertyId property, long value);

        /// <summary>
        /// Writes the property and value to the underlying stream with the appropriate format
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task WriteAsync(PropertyId property, ulong value);

        /// <summary>
        /// Writes the property and value to the underlying stream with the appropriate format
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task WriteAsync(PropertyId property, string value);

        /// <summary>
        /// Writes the property and value to the underlying stream with the appropriate format
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task WriteAsync(PropertyId property, double value);

        /// <summary>
        /// Writes the property and value to the underlying stream with the appropriate format
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task WriteAsync(PropertyId property, float value);

        /// <summary>
        /// Writes the property and value to the underlying stream with the appropriate format
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task WriteAsync(PropertyId property, Guid value);

        /// <summary>
        /// Writes the property and value to the underlying stream with the appropriate format
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task WriteAsync(PropertyId property, DateTime value);

        /// <summary>
        /// Create a new object writer
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        Task<ILucentObjectWriter> CreateObjectWriter(PropertyId property);

        /// <summary>
        /// Cast the current stream as an object writer
        /// </summary>
        /// <returns>The current stream as an object writer</returns>
        Task<ILucentObjectWriter> AsObjectWriter();

        /// <summary>
        /// Create a new array writer
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        Task<ILucentArrayWriter> CreateArrayWriter(PropertyId property);

        /// <summary>
        /// Flush the stream
        /// </summary>
        /// <returns></returns>
        Task Flush();
    }

    /// <summary>
    /// Specialized writer for objects
    /// </summary>
    public interface ILucentObjectWriter : ILucentWriter
    {
        /// <summary>
        /// Ends the object
        /// </summary>
        /// <returns></returns>
        Task EndObject();
    }

    /// <summary>
    /// Specialized writer for arrays
    /// </summary>
    public interface ILucentArrayWriter : IDisposable
    {

        /// <summary>
        /// Writes the value to the underlying stream with the appropriate format
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Task WriteAsync(int value);

        /// <summary>
        /// Writes the value to the underlying stream with the appropriate format
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>      
        Task WriteAsync(uint value);

        /// <summary>
        /// Writes the value to the underlying stream with the appropriate format
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Task WriteAsync(long value);

        /// <summary>
        /// Writes the value to the underlying stream with the appropriate format
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Task WriteAsync(ulong value);

        /// <summary>
        /// Writes the value to the underlying stream with the appropriate format
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Task WriteAsync(string value);

        /// <summary>
        /// Writes the value to the underlying stream with the appropriate format
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Task WriteAsync(double value);

        /// <summary>
        /// Writes the value to the underlying stream with the appropriate format
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Task WriteAsync(float value);

        /// <summary>
        /// Writes the value to the underlying stream with the appropriate format
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Task WriteAsync(Guid value);

        /// <summary>
        /// Writes the value to the underlying stream with the appropriate format
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Task WriteAsync(DateTime value);

        /// <summary>
        /// Writes the end of the array
        /// </summary>
        /// <returns></returns>
        Task WriteEnd();

        /// <summary>
        /// Flush the stream
        /// </summary>
        /// <returns></returns>
        Task Flush();

        /// <summary>
        /// Create a new object writer
        /// </summary>
        /// <returns></returns>
        ILucentObjectWriter CreateObjectWriter();

        /// <summary>
        /// Create a new array writer
        /// </summary>
        /// <returns></returns>
        ILucentArrayWriter CreateArrayWriter();

    }
}