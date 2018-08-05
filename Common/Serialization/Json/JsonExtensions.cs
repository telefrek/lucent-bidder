using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Lucent.Common.Serialization.Json
{
    /// <summary>
    /// Extensions for Json format reader/writer/utilities
    /// </summary>
    public static class JsonExtensions
    {
        /// <summary>
        /// Guard to ensure the token matches the expected type
        /// </summary>
        /// <param name="token"></param>
        /// <param name="expected"></param>
        public static void Guard(this JsonToken token, JsonToken expected)
        {
            if (token != expected)
                throw new SerializationException("Invalid JsonToken: {0}, expected {1}".FormatWith(token, expected));
        }

        /// <summary>
        /// Writes the property and name to the writer asynchronously
        /// </summary>
        /// <param name="writer">The JsonWriter to use</param>
        /// <param name="property">The name of the property</param>
        /// <param name="value">The value to write</param>
        /// <typeparam name="T">The type of value to write</typeparam>
        public static async Task WritePropertyAsync<T>(this JsonWriter writer, string property, T value)
        {
            await writer.WritePropertyNameAsync(property);
            if (object.Equals(value, default(T)))
                await writer.WriteNullAsync();
            else
            {
                var method = typeof(JsonWriter).GetMethod("WriteValueAsync", new Type[] { typeof(T), typeof(CancellationToken) });
                if (method != null)
                    await (Task)method.Invoke(writer, new object[] { value, CancellationToken.None });
                else
                    await writer.WriteNullAsync();
            }
        }
    }
}