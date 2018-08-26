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
}