#nullable enable
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AzisFood.DataEngine.Abstractions.Interfaces;

/// <summary>
///     Provides access to data
/// </summary>
public interface IDataAccess
{
    /// <summary>
    ///     Type of database to work with
    /// </summary>
    public string DbType { get; set; }

    /// <summary>
    ///     Get all entities
    /// </summary>
    /// <param name="token">Cancellation token</param>
    /// <returns>Items collection</returns>
    Task<IEnumerable<TRepoEntity>> GetAllAsync<TRepoEntity>(CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity;

    /// <summary>
    ///     Get single item
    /// </summary>
    /// <param name="id">Identifier of item</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Requested item</returns>
    Task<TRepoEntity?> GetAsync<TRepoEntity>(Guid id, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity;

    /// <summary>
    ///     Get filtered items
    /// </summary>
    /// <param name="filter">Filter expression</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Filtered items</returns>
    Task<IEnumerable<TRepoEntity>> GetAsync<TRepoEntity>(Expression<Func<TRepoEntity, bool>> filter,
        CancellationToken token = default) where TRepoEntity : class, IRepoEntity;

    /// <summary>
    ///     Create new item
    /// </summary>
    /// <param name="item">Item to create</param>
    /// <param name="token">Cancellation token</param>
    Task<TRepoEntity> CreateAsync<TRepoEntity>(TRepoEntity item, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity;

    /// <summary>
    ///     Update item
    /// </summary>
    /// <param name="id">Id of record</param>
    /// <param name="itemIn">New item state</param>
    /// <param name="token">Cancellation token</param>
    Task UpdateAsync<TRepoEntity>(Guid id, TRepoEntity itemIn, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity;

    /// <summary>
    ///     Remove item
    /// </summary>
    /// <param name="itemIn">Item to be removed</param>
    /// <param name="token">Cancellation token</param>
    Task RemoveAsync<TRepoEntity>(TRepoEntity itemIn, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity;

    /// <summary>
    ///     Remove item by id
    /// </summary>
    /// <param name="id">If od item to be removed</param>
    /// <param name="token">Cancellation token</param>
    Task RemoveAsync<TRepoEntity>(Guid id, CancellationToken token = default) where TRepoEntity : class, IRepoEntity;

    /// <summary>
    ///     Remove items by conditions
    /// </summary>
    /// <param name="filter">Filter expression</param>
    /// <param name="token">Cancellation token</param>
    Task RemoveAsync<TRepoEntity>(Expression<Func<TRepoEntity, bool>> filter, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity;

    /// <summary>
    ///     Remove items by ids
    /// </summary>
    /// <param name="ids">List of identifiers</param>
    /// <param name="token">Cancellation token</param>
    Task RemoveManyAsync<TRepoEntity>(Guid[] ids, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity;
}