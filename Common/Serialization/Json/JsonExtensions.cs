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
    }
}