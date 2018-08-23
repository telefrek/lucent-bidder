using System;
using System.Threading.Tasks;

namespace Lucent.Common.Serialization
{
    /// <summary>
    /// Interface used to read from a serialization stream
    /// </summary>
    public interface ISerializationStreamReader : IDisposable
    {
        /// <summary>
        /// Gets the current SerializationToken
        /// </summary>
        /// <returns>The type of token currently available</returns>
        SerializationToken Token { get; }

        /// <summary>
        /// Gets the current value at this position
        /// </summary>
        /// <returns>An object representing the current value at the position</returns>
        object Value { get; }

        /// <summary>
        /// Attempts to read the next token from the stream
        /// </summary>
        /// <returns>True if there are more tokens available in the stream</returns>
        bool HasNext();

        /// <summary>
        /// Skips the next token
        /// </summary>
        void Skip();

        /// <summary>
        /// Skips the next token asynchronously
        /// </summary>
        /// <returns></returns>
        Task SkipAsync();

        /// <summary>
        /// Attempts to read the next token from the stream asynchronously
        /// </summary>
        /// <returns>True if there are more tokens available in the stream</returns>
        Task<bool> HasNextAsync();

        /// <summary>
        /// Reads the next token as a dynamic object
        /// </summary>
        /// <returns>A dynamic object</returns>
        dynamic ReadDynamic();

        /// <summary>
        /// Reads the next token as a dynamic object asynchronously
        /// </summary>
        /// <returns>A dynamic object</returns>
        Task<dynamic> ReadDynamicAsync();

        /// <summary>
        /// Reads the next token as the given type
        /// </summary>
        /// <typeparam name="T">The type of object to read</typeparam>
        /// <returns>An object of the given type</returns>
        T ReadAs<T>() where T : new();

        /// <summary>
        /// Reads the next token as the given type asynchronously
        /// </summary>
        /// <typeparam name="T">The type of object to read</typeparam>
        /// <returns>An object of the given type</returns>
        Task<T> ReadAsAsync<T>() where T : new();

        /// <summary>
        /// Reads the next token as an array of dynamic objects
        /// </summary>
        /// <returns>An array of 0 or more dynamic objects</returns>
        dynamic[] ReadDynamicArray();

        /// <summary>
        /// Reads the next token as an array of dynamic objects asynchronously
        /// </summary>
        /// <returns>An array of 0 or more dynamic objects</returns>
        Task<dynamic[]> ReadDynamicArrayAsync();

        /// <summary>
        /// Reads the next token as an array of the given type
        /// </summary>
        /// <typeparam name="T">The type of object to read</typeparam>
        /// <returns>An array of 0 or more objects of the given type</returns>
        T[] ReadAsArray<T>() where T : new();

        /// <summary>
        /// Reads the next token as an array of the given type asynchronously
        /// </summary>
        /// <typeparam name="T">The type of object to read</typeparam>
        /// <returns>An array of 0 or more objects of the given type</returns>
        Task<T[]> ReadAsArrayAsync<T>() where T : new();

        /// <summary>
        /// Reads the next token as a boolean value
        /// </summary>
        /// <returns>A boolean value</returns>
        bool ReadBoolean();

        /// <summary>
        /// Reads the next token as a boolean value asynchronously
        /// </summary>
        /// <returns>A boolean value</returns>
        Task<bool> ReadBooleanAsync();

        /// <summary>
        /// Reads the next token as a double
        /// </summary>
        /// <returns>A double value</returns>
        double ReadDouble();

        /// <summary>
        /// Reads the next token as a double asynchronously
        /// </summary>
        /// <returns>A double value</returns>
        Task<double> ReadDoubleAsync();

        /// <summary>
        /// Reads the next token as a single
        /// </summary>
        /// <returns>A float value</returns>
        float ReadSingle();

        /// <summary>
        /// Reads the next token as a single asynchronously
        /// </summary>
        /// <returns>A float value</returns>
        Task<float> ReadSingleAsync();

        /// <summary>
        /// Reads the next token as a signed integer
        /// </summary>
        /// <returns>A signed integer value</returns>
        int ReadInt();

        /// <summary>
        /// Reads the next token as a signed integer asynchronously
        /// </summary>
        /// <returns>A signed integer value</returns>
        Task<int> ReadIntAsync();

        /// <summary>
        /// Reads the next token as an unsigned integer
        /// </summary>
        /// <returns>An unsigned integer value</returns>
        uint ReadUInt();

        /// <summary>
        /// Reads the next token as an unsigned integer asynchronously
        /// </summary>
        /// <returns>An unsigned integer value</returns>
        Task<uint> ReadUIntAsync();

        /// <summary>
        /// Reads the next token as a signed long
        /// </summary>
        /// <returns>A signed long value</returns>
        long ReadLong();

        /// <summary>
        /// Reads the next token as a signed long asynchronously
        /// </summary>
        /// <returns>A signed long value</returns>
        Task<long> ReadLongAsync();

        /// <summary>
        /// Reads the next token as an unsigned long
        /// </summary>
        /// <returns>An unsigned long value</returns>
        ulong ReadULong();

        /// <summary>
        /// Reads the next token as an unsigned long asynchronously
        /// </summary>
        /// <returns>An unsigned long value</returns>
        Task<ulong> ReadULongAsync();

        /// <summary>
        /// Reads the next token as a string
        /// </summary>
        /// <returns>A string value of 0 or more characters</returns>
        string ReadString();

        /// <summary>
        /// Reads the next token as a string asynchronously
        /// </summary>
        /// <returns>A string value of 0 or more characters</returns>
        Task<string> ReadStringAsync();

        /// <summary>
        /// Reads the next token as a string array
        /// </summary>
        /// <returns>An array of 0 or more strings</returns>
        string[] ReadStringArray();

        /// <summary>
        /// Reads the next token as a string array asynchronously
        /// </summary>
        /// <returns>An array of 0 or more strings</returns>
        Task<string[]> ReadStringArrayAsync();

        DateTime ReadDateTime();
        Task<DateTime> ReadDateTimeAsync();

        Guid ReadGuid();
        Task<Guid> ReadGuidAsync();
    }
}