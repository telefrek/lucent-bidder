namespace Lucent.Common.Messaging
{
    /// <summary>
    /// Represents the valid message processing states
    /// </summary>
    public enum MessageState
    {
        /// <value>No mapped state</value>
        Unknown = 0,
        /// <value>The message is invalid</value>
        Invalid = 1,
        /// <value>The message is being processed</value>
        InProgress = 2,
        /// <value>The message is completed</value>
        Completed = 3,
        /// <value>The message is deferred for later processing</value>
        Deferred = 4,
    }
}