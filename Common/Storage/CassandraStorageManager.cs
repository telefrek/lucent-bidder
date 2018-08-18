using Cassandra;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// Manager for Cassandra storage
    /// </summary>
    internal class CassandraStorageManager : IStorageManager
    {
        ICluster _cluster;
        ISession _session;

        public CassandraStorageManager()
        {
            _cluster = new CassandraConnectionStringBuilder
            {
                Username = "test",
                Password = "test",
                Port = 9042,
                
            }.MakeClusterBuilder().AddContactPoint("localhost").Build();
            _session = _cluster.Connect("test");
        }

        public ILucentRepository<T> GetRepository<T>() where T : new() => new CassandraRepository<T>(_session);
    }
}