using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AzisFood.DataEngine.Abstractions.Interfaces
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
        public Task<TEntity> GetHashAsync(Guid id, CancellationToken token = default);

        /// <summary>
        /// Get item by id async from hashset by supplied filter
        /// </summary>
        /// <param name="filter">Condition for filter</param>
        /// <param name="token">Token for operation cancel</param>
        /// <returns>Items matching condition</returns>
        Task<IEnumerable<TEntity>> GetHashAsync(Expression<Func<TEntity, bool>> filter,
            CancellationToken token = default);
    }
}