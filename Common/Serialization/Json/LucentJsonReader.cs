using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lucent.Common.Serialization.Json
{
    /// <summary>
    /// Protobuf reader
    /// </summary>
    public class LucentJsonReader : ILucentReader
    {
        readonly JsonReader jsonReader;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="target"></param>
        /// <param name="leaveOpen"></param>
        public LucentJsonReader(Stream target, bool leaveOpen)
            => jsonReader = new JsonTextReader(new StreamReader(target, Encoding.UTF8, false, 4096, leaveOpen));

        /// <inheritdoc/>
        public SerializationFormat Format { get => SerializationFormat.JSON; }

        /// <inheritdoc/>
        public async Task<ILucentArrayReader> GetArrayReader()
        {
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
        public async Task<Guid> NextGuid() => (await jsonReader.ReadAsStringAsync()).DecodeGuid();

        /// <inheritdoc/>
        public async Task<DateTime> NextDateTime() => DateTime.FromFileTimeUtc(Convert.ToInt64((await jsonReader.ReadAsDoubleAsync()).GetValueOrDefault()));

        /// <inheritdoc/>
        public async Task<TEnum> NextEnum<TEnum>() => (TEnum)Enum.ToObject(typeof(TEnum), (await jsonReader.ReadAsInt32Async()).GetValueOrDefault());

        /// <inheritdoc/>
        public async Task<byte[]> NextObjBytes()
        {
            var obj = await JObject.LoadAsync(jsonReader);
            if (obj != null)
                return Encoding.UTF8.GetBytes(obj.ToString(Formatting.None));

            return Encoding.UTF8.GetBytes("{}");
        }

        /// <inheritdoc/>
        public async Task Skip() => await jsonReader.SkipAsync();

        /// <inheritdoc/>
        public void Dispose() => jsonReader.Close();
    }
}