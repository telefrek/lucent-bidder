namespace Lucent.Common.Storage
{
    /// <summary>
    /// Storage key from a string
    /// </summary>
    public class StringStorageKey : IStorageKey
    {
        string _value;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="value"></param>
        public StringStorageKey(string value) => _value = value;

        /// <inheritdoc />
        public int CompareTo(object obj)
        {
            if (obj is string)
                return _value.CompareTo(obj);
            if (obj is StringStorageKey)
                return _value.CompareTo((obj as StringStorageKey)._value);
            if (obj is IStorageKey)
                return _value.CompareTo((obj as IStorageKey).ToString());

            return _value.CompareTo(obj.ToString());
        }

        /// <inheritdoc />
        public object[] RawValue() => new object[] { _value };

        /// <summary>
        /// Implicit operator for use in code
        /// </summary>
        /// <param name="value">The string to transform into a storage key</param>
        public static implicit operator StringStorageKey(string value) => new StringStorageKey(value);
    }
}