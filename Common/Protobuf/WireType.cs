namespace Lucent.Common.Protobuf
{
    /// <summary>
    /// The on the wire encoding type for a message
    /// </summary>
    public enum WireType
    {
        /// <value>
        /// Flag for variable size integers
        /// </value>
        VARINT = 0,
        /// <value>
        /// Flag for 64 bit values
        /// </value>
        FIXED_64 = 1,
        /// <value>
        /// Flag for length encoded values (strings, other objectss)
        /// </value>
        LEN_ENCODED = 2,
        /// <value>
        /// Flag for starting a group of values
        /// </value>
        START_GROUP = 3,
        /// <value>
        /// Flag for ending the grouped values
        /// </value>
        END_GROUP = 4,
        /// <value>
        /// Flag for 32 bit value
        /// </value>
        FIXED_32 = 5,

        /// <value>
        /// Flag for untranslated/invalid values
        /// </value>
        UNKNOWN = int.MaxValue,
    }
}