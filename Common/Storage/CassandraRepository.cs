using System.Threading.Tasks;
using Cassandra;
using Newtonsoft.Json.Linq;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// Internal Cassandra storage repository
    /// </summary>
    /// <typeparam name="T">The type of object to store in cassandra</typeparam>
    internal class CassandraRepository<T> : ILucentRepository<T>
        where T : new()
    {
        ISession _session;

        public CassandraRepository(ISession session)
        {
            _session = session;
        }

        public async Task<T> Get<K>(K key)
        {
            try
            {
                var stmt = new SimpleStatement("SELECT * FROM table1 WHERE id={0}".FormatWith(key));
                using (var rowSet = await _session.ExecuteAsync(stmt))
                {
                    var numRows = 0;
                    using (var rowEnum = rowSet.GetEnumerator())
                    {
                        while (!rowSet.IsFullyFetched)
                        {
                            if ((numRows = rowSet.GetAvailableWithoutFetching()) > 0)
                            {
                                for (var i = 0; i < numRows && rowEnum.MoveNext(); ++i)
                                {
                                    var row = rowEnum.Current;
                                    var id = row.GetValue<K>("id");

                                    var contents = row.GetValue<string>("contents");

                                    return new T();
                                }
                            }
                            else
                                await rowSet.FetchMoreResultsAsync();
                        }

                        while (rowEnum.MoveNext())
                        {
                            var row = rowEnum.Current;
                            var id = row.GetValue<K>("id");

                            var contents = row.GetValue<string>("contents");

                            return new T();
                        }
                    }
                }
            }
            catch
            {

            }

            return default(T);
        }

        public async Task<bool> TryInsert(T obj)
        {
            try
            {
                var stmt = new SimpleStatement("INSERT INTO table1 (id, contents) IF NOT EXISTS VALUES({0}, '{1}')".FormatWith(obj.GetType().GetProperty("Id").GetValue(obj), JObject.FromObject(obj).ToString()));
                await _session.ExecuteAsync(stmt);
                return true;
            }
            catch
            {

            }

            return false;
        }

        public async Task<bool> TryRemove(T obj)
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> TryUpdate(T obj)
        {
            throw new System.NotImplementedException();
        }
    }
}