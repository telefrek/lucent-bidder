using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Lucent.Common.Serialization.Json
{
    /// <summary>
    /// Writes objects on a json stream
    /// </summary>
    public class LucentJsonObjectWriter : ILucentObjectWriter
    {
        readonly JsonWriter _jsonWriter;
        readonly JsonFormat _format;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="jsonWriter"></param>
        /// <param name="format"></param>
        public LucentJsonObjectWriter(JsonWriter jsonWriter, JsonFormat format = default(JsonFormat)) 
        {
            _jsonWriter = jsonWriter;
            _format = format;
        }

        /// <inheritdoc/>
        public SerializationFormat Format { get { return SerializationFormat.PROTOBUF; } }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, bool value)
        {
            if (!value.IsNullOrDefault())
            {
                await _jsonWriter.WritePropertyNameAsync(property.Name);
                await (_format == JsonFormat.OpenRTB ? _jsonWriter.WriteValueAsync(value ? 1 : 0) : _jsonWriter.WriteValueAsync(value));
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, int value)
        {
            if (!value.IsNullOrDefault())
            {
                await _jsonWriter.WritePropertyNameAsync(property.Name);
                await _jsonWriter.WriteValueAsync(value);
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, uint value)
        {
            if (!value.IsNullOrDefault())
            {
                await _jsonWriter.WritePropertyNameAsync(property.Name);
                await _jsonWriter.WriteValueAsync(value);
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, long value)
        {
            if (!value.IsNullOrDefault())
            {
                await _jsonWriter.WritePropertyNameAsync(property.Name);
                await _jsonWriter.WriteValueAsync(value);
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, ulong value)
        {
            if (!value.IsNullOrDefault())
            {
                await _jsonWriter.WritePropertyNameAsync(property.Name);
                await _jsonWriter.WriteValueAsync(value);
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, string value)
        {
            if (!value.IsNullOrDefault())
            {
                await _jsonWriter.WritePropertyNameAsync(property.Name);
                await _jsonWriter.WriteValueAsync(value);
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, double value)
        {
            if (!value.IsNullOrDefault())
            {
                await _jsonWriter.WritePropertyNameAsync(property.Name);
                await _jsonWriter.WriteValueAsync(value);
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, float value)
        {
            if (!value.IsNullOrDefault())
            {
                await _jsonWriter.WritePropertyNameAsync(property.Name);
                await _jsonWriter.WriteValueAsync(value);
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, Guid value)
        {
            if (!value.IsNullOrDefault())
            {
                await _jsonWriter.WritePropertyNameAsync(property.Name);
                await _jsonWriter.WriteValueAsync(value.EncodeGuid());
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, DateTime value)
        {
            if (!value.IsNullOrDefault())
            {
                await _jsonWriter.WritePropertyNameAsync(property.Name);
                await _jsonWriter.WriteValueAsync(value.ToFileTimeUtc());
            }
        }

        /// <inheritdoc/>
        public async Task<ILucentObjectWriter> CreateObjectWriter(PropertyId property)
        {
            await _jsonWriter.WritePropertyNameAsync(property.Name);
            await _jsonWriter.WriteStartObjectAsync();
            return new LucentJsonObjectWriter(_jsonWriter);
        }

        /// <inheritdoc/>
        public async Task<ILucentArrayWriter> CreateArrayWriter(PropertyId property)
        {
            await _jsonWriter.WritePropertyNameAsync(property.Name);
            await _jsonWriter.WriteStartArrayAsync();
            return new LucentJsonArrayWriter(_jsonWriter);
        }

        /// <inheritdoc/>
        public async Task EndObject() => await _jsonWriter.WriteEndObjectAsync();

        /// <inheritdoc/>
        public void Dispose() { }

        /// <inheritdoc/>
        public async Task Flush() => await _jsonWriter.FlushAsync();
    }
}