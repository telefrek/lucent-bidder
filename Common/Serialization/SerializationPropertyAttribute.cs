using System;

namespace Lucent.Common.Serialization
{
    /// <summary>
    /// Attribute for marking serialization information
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class SerializationPropertyAttribute : Attribute
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SerializationPropertyAttribute(ulong id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// The numeric identifier
        /// </summary>
        /// <value></value>
        public ulong Id { get; set; }

        /// <summary>
        /// The named string
        /// </summary>
        /// <value></value>
        public string Name { get; set; }

        /// <summary>
        /// Optional override for checking type override
        /// </summary>
        /// <value></value>
        public Type AsType { get; set; }
    }
}