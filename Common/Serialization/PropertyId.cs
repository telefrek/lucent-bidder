namespace Lucent.Common.Serialization
{
    /// <summary>
    /// Wrapper for property serialization information
    /// </summary>
    public class PropertyId
    {
        /// <summary>
        /// Gets/Sets the name
        /// </summary>
        /// <value></value>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets/Sets the Id
        /// </summary>
        /// <value></value>
        public ulong Id { get; set; }

        /// <summary>
        /// Implicit conversion
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator PropertyId(ulong value) => new PropertyId { Id = value };

        /// <summary>
        /// Implicit conversion
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator PropertyId(string value) => new PropertyId { Name = value };
    }
}