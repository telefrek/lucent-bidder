using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Lucent.Common.Serialization.Json
{
    /// <summary>
    /// Reads a protobuf array
    /// </summary>
    public class LucentJsonArrayReader : ILucentArrayReader
    {
        volatile int _counter = 1;
        readonly JsonReader jsonReader;
        volatile bool _readEnd = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="reader"></param>
        public LucentJsonArrayReader(JsonReader reader)
            => jsonReader = reader;

        /// <inheritdoc/>
        public SerializationFormat Format { get => SerializationFormat.JSON; }

        /// <inheritdoc/>
        public async Task<ILucentArrayReader> GetArrayReader()
        {
            _counter++;
            await jsonReader.ReadAsync();
            return new LucentJsonArrayReader(jsonReader);
        }

        /// <inheritdoc/>
        public async Task<ILucentObjectReader> GetObjectReader()
        {
            await jsonReader.ReadAsync();
            return new LucentJsonObjectReader(jsonReader);
        }

        /// <inheritdoc/>
        public Task<bool> IsComplete()
        {
            if (jsonReader.TokenType == JsonToken.EndArray) _counter--;
            return Task.FromResult(_counter == 0);
        }

        /// <inheritdoc/>
        public async Task<PropertyId> NextAsync() =>
            await jsonReader.ReadAsync() && jsonReader.TokenType == JsonToken.PropertyName ?
                    new PropertyId { Name = (string)jsonReader.Value } : null;

        /// <inheritdoc/>
        public async Task<bool> NextBoolean() => (await jsonReader.ReadAsBooleanAsync()).GetValueOrDefault();

        /// <inheritdoc/>
        public async Task<double> NextDouble() => (await jsonReader.ReadAsDoubleAsync()).GetValueOrDefault();

        /// <inheritdoc/>
        public async Task<int> NextInt() => (await jsonReader.ReadAsInt32Async()).GetValueOrDefault();

        /// <inheritdoc/>
        public async Task<long> NextLong() => Convert.ToInt64((await jsonReader.ReadAsDoubleAsync()).GetValueOrDefault());

        /// <inheritdoc/>
        public async Task<float> NextSingle() => Convert.ToSingle((await jsonReader.ReadAsDecimalAsync()).GetValueOrDefault());

        /// <inheritdoc/>
        public async Task<string> NextString() => await jsonReader.ReadAsStringAsync();

        /// <inheritdoc/>
        public async Task<uint> NextUInt() => Convert.ToUInt32((await jsonReader.ReadAsInt32Async()).GetValueOrDefault());

        /// <inheritdoc/>
        public async Task<ulong> NextULong() => Convert.ToUInt64((await jsonReader.ReadAsDoubleAsync()).GetValueOrDefault());

        /// <inheritdoc/>
        public async Task<Guid> NextGuid() => Guid.Parse(await jsonReader.ReadAsStringAsync());

        /// <inheritdoc/>
        public async Task<DateTime> NextDateTime() => DateTime.FromFileTimeUtc(Convert.ToInt64((await jsonReader.ReadAsDoubleAsync()).GetValueOrDefault()));

        /// <inheritdoc/>
        public async Task<TEnum> NextEnum<TEnum>() => (TEnum)Enum.ToObject(typeof(TEnum), (await jsonReader.ReadAsInt32Async()).GetValueOrDefault());

        /// <inheritdoc/>
        public async Task Skip() => await jsonReader.SkipAsync();

        /// <inheritdoc/>
        public void Dispose() { }
    }
}