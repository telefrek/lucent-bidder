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
        readonly JsonWriter jsonWriter;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="target"></param>
        public LucentJsonObjectWriter(JsonWriter target) => jsonWriter = target;

        /// <inheritdoc/>
        public SerializationFormat Format { get { return SerializationFormat.PROTOBUF; } }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, int value)
        {
            if (!value.IsNullOrDefault())
            {
                await jsonWriter.WritePropertyNameAsync(property.Name);
                await jsonWriter.WriteValueAsync(value);
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, uint value)
        {
            if (!value.IsNullOrDefault())
            {
                await jsonWriter.WritePropertyNameAsync(property.Name);
                await jsonWriter.WriteValueAsync(value);
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, long value)
        {
            if (!value.IsNullOrDefault())
            {
                await jsonWriter.WritePropertyNameAsync(property.Name);
                await jsonWriter.WriteValueAsync(value);
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, ulong value)
        {
            if (!value.IsNullOrDefault())
            {
                await jsonWriter.WritePropertyNameAsync(property.Name);
                await jsonWriter.WriteValueAsync(value);
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, string value)
        {
            if (!value.IsNullOrDefault())
            {
                await jsonWriter.WritePropertyNameAsync(property.Name);
                await jsonWriter.WriteValueAsync(value);
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, double value)
        {
            if (!value.IsNullOrDefault())
            {
                await jsonWriter.WritePropertyNameAsync(property.Name);
                await jsonWriter.WriteValueAsync(value);
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, float value)
        {
            if (!value.IsNullOrDefault())
            {
                await jsonWriter.WritePropertyNameAsync(property.Name);
                await jsonWriter.WriteValueAsync(value);
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, Guid value)
        {
            if (!value.IsNullOrDefault())
            {
                await jsonWriter.WritePropertyNameAsync(property.Name);
                await jsonWriter.WriteValueAsync(value.EncodeGuid());
            }
        }

        /// <inheritdoc/>
        public async Task WriteAsync(PropertyId property, DateTime value)
        {
            if (!value.IsNullOrDefault())
            {
                await jsonWriter.WritePropertyNameAsync(property.Name);
                await jsonWriter.WriteValueAsync(value.ToFileTimeUtc());
            }
        }

        /// <inheritdoc/>
        public async Task<ILucentObjectWriter> CreateObjectWriter(PropertyId property)
        {
            await jsonWriter.WritePropertyNameAsync(property.Name);
            await jsonWriter.WriteStartObjectAsync();
            return new LucentJsonObjectWriter(jsonWriter);
        }

        /// <inheritdoc/>
        public async Task<ILucentArrayWriter> CreateArrayWriter(PropertyId property)
        {
            await jsonWriter.WritePropertyNameAsync(property.Name);
            await jsonWriter.WriteStartArrayAsync();
            return new LucentJsonArrayWriter(jsonWriter);
        }

        /// <inheritdoc/>
        public async Task EndObject() => await jsonWriter.WriteEndObjectAsync();

        /// <inheritdoc/>
        public void Dispose() { }

        /// <inheritdoc/>
        public async Task Flush() => await jsonWriter.FlushAsync();
    }
}