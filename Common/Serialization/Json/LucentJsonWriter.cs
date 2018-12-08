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
        readonly JsonWriter jsonWriter;

        /// <summary>
        /// Default construtroe
        /// </summary>
        /// <param name="target"></param>
        /// <param name="leaveOpen"></param>
        public LucentJsonWriter(Stream target, bool leaveOpen = false)
        {
            jsonWriter = new JsonTextWriter(new StreamWriter(target, Encoding.UTF8, 4096, leaveOpen));
            jsonWriter.WriteStartObject();
        }

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
        public void Dispose() 
        {
            jsonWriter.WriteEndObject();
            jsonWriter.Close();
        }

        /// <inheritdoc/>
        public async Task Flush() => await jsonWriter.FlushAsync();
    }
}