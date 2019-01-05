using System;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// Represents a storage key (singluar, composite, clustered, etc.)
    /// </summary>
    public interface IStorageKey : IComparable
    {
        /// <summary>
        /// Check if the key is composite or singular
        /// </summary>
        /// <value></value>
        bool IsComposite { get; }

        /// <summary>
        /// Get the key value as a string
        /// </summary>
        /// <returns></returns>
        string StringValue();

        /// <summary>
        /// Get the raw values that make up the key
        /// </summary>
        /// <returns></returns>
        object[] RawValue();
    }
}