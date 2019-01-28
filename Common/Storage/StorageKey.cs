using System;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// Represents a storage key (singluar, composite, clustered, etc.)
    /// </summary>
    public class StorageKey : IComparable
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public StorageKey()
        {

        }

        /// <summary>
        /// Get the raw values that make up the key
        /// </summary>
        /// <returns></returns>
        public virtual object[] RawValue() => null;

        /// <summary>
        /// Must be able to parse from a string
        /// </summary>
        /// <param name="value"></param>
        public virtual void Parse(string value) => throw new InvalidCastException();

        /// <inheritdoc/>
        public virtual int CompareTo(object obj) => throw new InvalidCastException();
    }
}