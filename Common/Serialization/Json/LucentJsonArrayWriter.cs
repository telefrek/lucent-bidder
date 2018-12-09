using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Lucent.Common.Serialization.Json
{
    /// <summary>
    /// Json Writer
    /// </summary>
    public class LucentJsonArrayWriter : ILucentArrayWriter
    {
        JsonWriter jsonWriter;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="writer">The current writer</param>
        public LucentJsonArrayWriter(JsonWriter writer) => jsonWriter = writer;

        /// <inheritdoc/>
        public async Task WriteAsync(int value) => await jsonWriter.WriteValueAsync(value);

        /// <inheritdoc/>
        public async Task WriteAsync(uint value) => await jsonWriter.WriteValueAsync(value);

        /// <inheritdoc/>
        public async Task WriteAsync(long value) => await jsonWriter.WriteValueAsync(value);

        /// <inheritdoc/>
        public async Task WriteAsync(ulong value) => await jsonWriter.WriteValueAsync(value);

        /// <inheritdoc/>
        public async Task WriteAsync(string value) => await jsonWriter.WriteValueAsync(value);

        /// <inheritdoc/>
        public async Task WriteAsync(double value) => await jsonWriter.WriteValueAsync(value);

        /// <inheritdoc/>
        public async Task WriteAsync(float value) => await jsonWriter.WriteValueAsync(value);

        /// <inheritdoc/>
        public async Task WriteAsync(Guid value) => await jsonWriter.WriteValueAsync(value.EncodeGuid());

        /// <inheritdoc/>
        public async Task WriteAsync(DateTime value) => await jsonWriter.WriteValueAsync(value.ToFileTimeUtc());

        /// <inheritdoc/>
        public async Task WriteEnd() => await jsonWriter.WriteEndArrayAsync();

        /// <inheritdoc/>
        public async Task<ILucentObjectWriter> CreateObjectWriter()
        {
            await jsonWriter.WriteStartObjectAsync();
            return new LucentJsonObjectWriter(jsonWriter);
        }

        /// <inheritdoc/>
        public async Task<ILucentArrayWriter> CreateArrayWriter()
        {
            await jsonWriter.WriteStartArrayAsync();
            return new LucentJsonArrayWriter(jsonWriter);
        }

        /// <inheritdoc/>
        public void Dispose() { }

        /// <inheritdoc/>
        public async Task Flush() => await jsonWriter.FlushAsync();
    }
}