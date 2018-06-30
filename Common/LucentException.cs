using System;
using System.Runtime.Serialization;

namespace Lucent.Common
{
    /// <summary>
    /// Base exception for the Lucent libraries
    /// </summary>
    public class LucentException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public LucentException()
        {
        }

        /// <summary>
        /// Constructor with a custom message
        /// </summary>
        /// <param name="message">The message to pass with the exception</param>
        public LucentException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor with a custom message and root cause
        /// </summary>
        /// <param name="message">The message to pass with the exception</param>
        /// <param name="innerException">The underlying cause of this exception</param>
        public LucentException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}