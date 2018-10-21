using Lucent.Common.Serialization;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// POCO Configuration for cassanddra
    /// </summary>
    public class CassandraConfiguration
    {
        /// <summary>
        /// The endpoint to connect to
        /// </summary>
        /// <value></value>
        public string Endpoint { get; set; } = "localhost";

        /// <summary>
        /// The keyspace to use
        /// </summary>
        /// <value></value>
        public string Keyspace { get; set; } = "test";

        /// <summary>
        /// The user to connect with
        /// </summary>
        /// <value></value>
        public string User { get; set; } = "test";

        /// <summary>
        /// The credentials for authenticating the user
        /// </summary>
        /// <value></value>
        public string Credentials { get; set; } = "test";

        /// <summary>
        /// The default serialization format to use
        /// </summary>
        /// <value></value>
        public SerializationFormat Format { get; set; } = SerializationFormat.PROTOBUF;
    }
}