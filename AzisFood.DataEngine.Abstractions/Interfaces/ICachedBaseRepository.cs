using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AzisFood.DataEngine.Interfaces
{
    public interface ICachedBaseRepository<TEntity> : IBaseRepository<TEntity>
    {
        /// <summary>
        /// Get items async from hashset
        /// </summary>
        /// <param name="token">Token for operation cancel</param>
        /// <returns>Collection of item</returns>
        public Task<IEnumerable<TEntity>> GetHashAsync(CancellationToken token = default);
        
        /// <summary>
        /// Get item by id async from hashset by id
        /// </summary>
        /// <param name="id">Identifier of item</param>
        /// <param name="token">Token for operation cancel</param>
        /// <returns>Item with supplied id</returns>
        public Task<TEntity> GetHashAsync(string id, CancellationToken token = default);
    }
}