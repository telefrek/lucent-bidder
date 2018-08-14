namespace Lucent.Common.Messaging
{
    /// <summary>
    /// Configuation POCO class for RabbitMQ configuration
    /// </summary>
    public class RabbitConfiguration
    {
        /// <summary>
        /// Gets/Sets the cluster hostname
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets/Sets the cluster username
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Gets/Sets the cluster user credentials
        /// </summary>
        public string Credentials { get; set; }
    }
}