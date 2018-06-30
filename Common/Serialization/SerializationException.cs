using System;
using System.Runtime.Serialization;

namespace Lucent.Common.Serialization
{
    /// <summary>
    /// Base exception raised for serialization issues
    /// </summary>
    public class SerializationException : LucentException
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SerializationException()
        {
        }

        /// <summary>
        /// Constructor with a custom message
        /// </summary>
        /// <param name="message">The message to pass with the exception</param>
        public SerializationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor with a custom message and root cause
        /// </summary>
        /// <param name="message">The message to pass with the exception</param>
        /// <param name="innerException">The underlying cause of this exception</param>
        public SerializationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}