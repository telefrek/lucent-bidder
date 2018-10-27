using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lucent.Common.Storage
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IClusteredRepository<T> : IStorageRepository<T>
        where T : IClusteredStorageEntity
    {
        /// <summary>
        /// Get a specific entry
        /// </summary>
        /// <param name="id"></param>
        /// <param name="secondaryId"></param>
        /// <returns></returns>
        Task<T> Get(string id, Guid secondaryId);

        /// <summary>
        /// Get all the entries for the cluster
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<List<T>> GetCluster(string id);
    }
}