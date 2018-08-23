namespace Lucent.Common.Storage
{
    public class CassandraConfiguration
    {
        public string Endpoint { get; set; } = "localhost";
        public string Keyspace { get; set; } = "test";
        public string User { get; set; } = "test";
        public string Credentials { get; set; } = "test";
    }
}