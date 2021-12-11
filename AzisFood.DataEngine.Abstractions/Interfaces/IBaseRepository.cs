using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AzisFood.DataEngine.Interfaces
{
    public interface IBaseRepository<TEntity>
    {
        /// <summary>
        /// Name of entity type
        /// </summary>
        public string RepoEntityName { get; init; }
        
        /// <summary>
        /// Get items async
        /// </summary>
        /// <returns>Collection of item</returns>
        public Task<IEnumerable<TEntity>> GetAsync(CancellationToken token = default);

        /// <summary>
        /// Get item by id
        /// </summary>
        /// <param name="id">Identifier of item</param>
        /// <param name="token">Token for operation cancel</param>
        /// <returns>Item with supplied id</returns>
        Task<TEntity> GetAsync(string id, CancellationToken token = default);

        /// <summary>
        /// Get items by condition
        /// </summary>
        /// <param name="filter">Condition for item filtering</param>
        /// <param name="token">Token for operation cancel</param>
        /// <returns>Items matching condition</returns>
        Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter, CancellationToken token = default);
        
        /// <summary>
        /// Insert new item
        /// </summary>
        /// <param name="item">Item to insert</param>
        /// <param name="token">Token for operation cancel</param>
        /// <returns>Inserted item</returns>
        Task<TEntity> CreateAsync(TEntity item, CancellationToken token = default);
        
        /// <summary>
        /// Update item by id
        /// </summary>
        /// <param name="id">Id of item</param>
        /// <param name="itemIn">New value</param>
        /// <param name="token">Token for operation cancel</param>
        Task UpdateAsync(string id, TEntity itemIn, CancellationToken token = default);
        
        /// <summary>
        /// Delete item
        /// </summary>
        /// <param name="itemIn">Value to delete</param>
        /// <param name="token">Token for operation cancel</param>
        Task RemoveAsync(TEntity itemIn, CancellationToken token = default);
        
        /// <summary>
        /// Delete item by id
        /// </summary>
        /// <param name="id">Id of item to delete</param>
        /// <param name="token">Token for operation cancel</param>
        Task RemoveAsync(string id, CancellationToken token = default);

        /// <summary>
        /// Delete items by condition
        /// </summary>
        /// <param name="filter">Condition for item filtering</param>
        /// <param name="token">Token for operation cancel</param>
        Task RemoveAsync(Expression<Func<TEntity, bool>> filter, CancellationToken token = default);
        
        /// <summary>
        /// Delete items with id in list
        /// </summary>
        /// <param name="ids">Ids of items to delete</param>
        /// <param name="token">Token for operation cancel</param>
        Task RemoveManyAsync(string[] ids, CancellationToken token = default);
    }
}