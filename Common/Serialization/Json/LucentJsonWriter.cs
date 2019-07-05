using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Lucent.Common.Serialization.Json
{
    /// <summary>
    /// Protobuf writer
    /// </summary>
    public class LucentJsonWriter : ILucentWriter
    {
        readonly JsonWriter _jsonWriter;
        readonly JsonFormat _format;

        internal LucentJsonWriter(JsonTextWriter jsonWriter, JsonFormat format = default(JsonFormat))
        {
            _jsonWriter = jsonWriter;
            _format = format;
        }

        /// <inheritdoc/>
        public SerializationFormat Format { get { return SerializationFormat.JSON; } }

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
                await _jsonWriter.WriteValueAsync(value.ToString());
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
        public async Task<ILucentObjectWriter> CreateObjectWriter()
        {
            await _jsonWriter.WriteStartObjectAsync();
            return new LucentJsonObjectWriter(_jsonWriter);
        }

        /// <inheritdoc/>
        public async Task<ILucentArrayWriter> CreateArrayWriter()
        {
            await _jsonWriter.WriteStartArrayAsync();
            return new LucentJsonArrayWriter(_jsonWriter);
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
        public void Dispose() => _jsonWriter.Close();

        /// <inheritdoc/>
        public async Task Flush() => await _jsonWriter.FlushAsync();
    }
}