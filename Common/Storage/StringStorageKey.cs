using System;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// Storage key from a string
    /// </summary>
    public class StringStorageKey : StorageKey
    {
        string _value;

        /// <summary>
        /// Default constructor
        /// </summary>
        public StringStorageKey() => _value = SequentialGuid.NextGuid().ToString();

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="value"></param>
        public StringStorageKey(string value) => _value = value;

        /// <inheritdoc/>
        public override string ToString() => _value;

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var ssk = obj as StringStorageKey;
            if (ssk != null) return _value.Equals(ssk._value);

            return _value.Equals(obj.ToString());
        }

        /// <inheritdoc/>
        public override int GetHashCode() => _value.GetHashCode();

        /// <inheritdoc />
        public override int CompareTo(object obj)
        {
            if (obj is string)
                return _value.CompareTo(obj);
            if (obj is StringStorageKey)
                return _value.CompareTo((obj as StringStorageKey)._value);
            if (obj is StorageKey)
                return _value.CompareTo((obj as StorageKey).ToString());

            return _value.CompareTo(obj.ToString());
        }

        /// <inheritdoc/>
        public override void Parse(string value) => _value = value;

        /// <inheritdoc />
        public override object[] RawValue() => new object[] { _value };

    }
}