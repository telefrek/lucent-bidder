using System;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// Represents a storage key (singluar, composite, clustered, etc.)
    /// </summary>
    public interface IStorageKey : IComparable
    {
        /// <summary>
        /// Get the raw values that make up the key
        /// </summary>
        /// <returns></returns>
        object[] RawValue();
    }
}