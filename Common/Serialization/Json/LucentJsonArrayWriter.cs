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
        readonly JsonWriter _jsonWriter;
        readonly JsonFormat _format;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="jsonWriter">The current writer</param>
        /// <param name="format"></param>
        internal LucentJsonArrayWriter(JsonWriter jsonWriter, JsonFormat format = default(JsonFormat)) 
        {
            _jsonWriter = jsonWriter;
            _format = format;
        }

        /// <inheritdoc/>
        public async Task WriteAsync(bool value) => await (_format == JsonFormat.OpenRTB ? _jsonWriter.WriteValueAsync(value ? 1 : 0) : _jsonWriter.WriteValueAsync(value));

        /// <inheritdoc/>
        public async Task WriteAsync(int value) => await _jsonWriter.WriteValueAsync(value);

        /// <inheritdoc/>
        public async Task WriteAsync(uint value) => await _jsonWriter.WriteValueAsync(value);

        /// <inheritdoc/>
        public async Task WriteAsync(long value) => await _jsonWriter.WriteValueAsync(value);

        /// <inheritdoc/>
        public async Task WriteAsync(ulong value) => await _jsonWriter.WriteValueAsync(value);

        /// <inheritdoc/>
        public async Task WriteAsync(string value) => await _jsonWriter.WriteValueAsync(value);

        /// <inheritdoc/>
        public async Task WriteAsync(double value) => await _jsonWriter.WriteValueAsync(value);

        /// <inheritdoc/>
        public async Task WriteAsync(float value) => await _jsonWriter.WriteValueAsync(value);

        /// <inheritdoc/>
        public async Task WriteAsync(Guid value) => await _jsonWriter.WriteValueAsync(value.EncodeGuid());

        /// <inheritdoc/>
        public async Task WriteAsync(DateTime value) => await _jsonWriter.WriteValueAsync(value.ToFileTimeUtc());

        /// <inheritdoc/>
        public async Task WriteEnd() => await _jsonWriter.WriteEndArrayAsync();

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
        public void Dispose() { }

        /// <inheritdoc/>
        public async Task Flush() => await _jsonWriter.FlushAsync();
    }
}