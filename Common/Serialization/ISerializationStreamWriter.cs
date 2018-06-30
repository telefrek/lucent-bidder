using System;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;

namespace Lucent.Common.Serialization
{
    /// <summary>
    /// Interface for writing to a serialization stream
    /// </summary>
    public interface ISerializationStreamWriter : IDisposable
    {
        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(ExpandoObject value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(ExpandoObject[] value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write<T>(T value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write<T>(T[] value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(bool value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(double value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(float value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(int value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(uint value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(long value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(ulong value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(string value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(string[] value);


        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(ExpandoObject value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(ExpandoObject[] value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync<T>(T value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync<T>(T[] value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(bool value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(double value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(float value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(int value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(uint value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(long value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(ulong value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(string value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(string[] value);
    }
}