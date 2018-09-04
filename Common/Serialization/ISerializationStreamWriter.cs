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
        /// Flushes the stream
        /// </summary>
        void Flush();

        /// <summary>
        /// Flushes the stream asynchronously
        /// </summary>
        /// <returns></returns>
        Task FlushAsync();

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write<T>(T value) where T : new();

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write<T>(T[] value) where T : new();

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
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(DateTime value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(Guid value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync<T>(T value) where T : new();

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync<T>(T[] value) where T : new();

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

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(DateTime value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(Guid value);

        /// <summary>
        /// 
        /// </summary>
        void StartObject();

        /// <summary>
        /// 
        /// </summary>
        void StartArray();

        /// <summary>
        /// 
        /// </summary>
        Task StartObjectAsync();

        /// <summary>
        /// 
        /// </summary>
        Task StartArrayAsync();

        /// <summary>
        /// 
        /// </summary>
        void EndObject();

        /// <summary>
        /// 
        /// </summary>
        void EndArray();

        /// <summary>
        /// 
        /// </summary>
        Task EndObjectAsync();

        /// <summary>
        /// 
        /// </summary>
        Task EndArrayAsync();

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write<T>(PropertyId id, T value) where T : new();

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write<T>(PropertyId id, T[] value) where T : new();

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(PropertyId id, bool value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(PropertyId id, double value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(PropertyId id, float value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(PropertyId id, int value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(PropertyId id, uint value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(PropertyId id, long value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(PropertyId id, ulong value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(PropertyId id, string value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(PropertyId id, string[] value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(PropertyId id, DateTime value);

        /// <summary>
        /// Writes the value to the stream
        /// </summary>
        /// <param name="value"></param>
        void Write(PropertyId id, Guid value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync<T>(PropertyId id, T value) where T : new();

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync<T>(PropertyId id, T[] value) where T : new();

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(PropertyId id, bool value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(PropertyId id, double value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(PropertyId id, float value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(PropertyId id, int value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(PropertyId id, uint value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(PropertyId id, long value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(PropertyId id, ulong value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(PropertyId id, string value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(PropertyId id, string[] value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(PropertyId id, DateTime value);

        /// <summary>
        /// Writes the value to the stream asynchronously
        /// </summary>
        /// <param name="value"></param>
        Task WriteAsync(PropertyId id, Guid value);

    }
}