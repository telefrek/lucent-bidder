namespace Lucent.Common.Serialization
{
    /// <summary>
    /// Token types for serialization
    /// </summary>
    public enum SerializationToken
    {
        /// <value>
        /// Unknown/bad type
        /// </value>
        Unknown = 0,
        /// <value>
        /// Complex Object
        /// </value>
        Object = 1,
        /// <value>
        /// Array of Objects/Values
        /// </value>
        Array = 2,
        /// <value>
        /// Property name
        /// </value>
        Property = 3,
        /// <value>
        /// Raw value type, up to reader to cast appropriately
        /// </value>
        Value = 4,
        /// <value>
        /// End of Stream
        /// </value>
        EndOfStream = 5,
        /// <value>
        /// Null indicator
        /// </value>
        Null = 6,
    }

    /// <summary>
    /// Class to manage serialization token extension methods
    /// </summary>
    public static class SerializationTokenExtensions
    {
        /// <summary>
        /// Guards to ensure the token is of the expected type
        /// </summary>
        /// <param name="token">The token to validate</param>
        /// <param name="expected">The expected type that must match</param>
        public static void Guard(this SerializationToken token, SerializationToken expected)
        {
            if (token != expected)
                throw new SerializationException("Invalid SerializationToken: {0}, expected {1}".FormatWith(token, expected));
        }
    }
}