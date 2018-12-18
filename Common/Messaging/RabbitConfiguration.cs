using System.Collections.Generic;

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

        /// <summary>
        /// Virtual host
        /// </summary>
        /// <value></value>
        public string VHost { get; set; } = "/";

        /// <summary>
        /// Gets/Sets the cluster info
        /// </summary>
        public Dictionary<string, RabbitCluster> Clusters { get; set; } = new Dictionary<string, RabbitCluster>();
    }

    /// <summary>
    /// Represents a cluster
    /// </summary>
    public class RabbitCluster
    {
        /// <summary>
        /// Gets/Sets the cluster hostname
        /// </summary>
        /// <value></value>
        public string Host { get; set; }

        /// <summary>
        /// Gets/Sets the cluster username
        /// </summary>
        /// <value></value>
        public string User { get; set; }

        /// <summary>
        /// Gets/Sets the cluster user credentials
        /// </summary>
        /// <value></value>
        public string Credentials { get; set; }

        /// <summary>
        /// Virtual host
        /// </summary>
        /// <value></value>
        public string VHost { get; set; } = "/";
    }
}