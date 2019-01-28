using System;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// Storage key from a string
    /// </summary>
    public class GuidStorageKey : StorageKey
    {
        Guid _value;

        /// <summary>
        /// Default constructor
        /// </summary>
        public GuidStorageKey() => _value = Guid.Empty;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="value"></param>
        public GuidStorageKey(Guid value) => _value = value;

        /// <inheritdoc/>
        public override string ToString() => _value.ToString();

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var gsk = obj as GuidStorageKey;
            if(gsk != null) return _value.Equals(gsk._value);

            return false;
        }
        
        /// <inheritdoc/>
        public override int GetHashCode() => _value.GetHashCode();

        /// <inheritdoc />
        public override int CompareTo(object obj)
        {
            if (obj is Guid)
                return _value.CompareTo(obj);
            if (obj is GuidStorageKey)
                return _value.CompareTo((obj as GuidStorageKey)._value);
            if (obj is StorageKey)
                return _value.CompareTo((obj as StorageKey).ToString());

            return _value.CompareTo(obj.ToString());
        }
        
        /// <inheritdoc/>
        public override void Parse(string value) => _value = Guid.Parse(value);

        /// <inheritdoc/>
        public override object[] RawValue() => new object[] { _value };
    }
}