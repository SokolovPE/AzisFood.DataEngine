#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzisFood.DataEngine.Abstractions.Interfaces;

/// <summary>
///     Adapter of cache provider
/// </summary>
public interface ICacheAdapter
{
    /// <summary>
    ///     Get collection of elements from hashset
    /// </summary>
    Task<IEnumerable<TEntity>?> GetCollectionFromHashAsync<TEntity>() where TEntity : class, IRepoEntity, new();

    /// <summary>
    ///     Get single element from hashset
    /// </summary>
    /// <param name="key">Entity key</param>
    Task<TEntity> GetSingleFromHashAsync<TEntity>(Guid key) where TEntity : class, IRepoEntity, new();

    /// <summary>
    ///     Get collection of elements from collection stored in single key
    /// </summary>
    /// <param name="entityName">Name of entity</param>
    Task<IEnumerable<TEntity>> GetFromSingleKeyAsync<TEntity>(string entityName)
        where TEntity : class, IRepoEntity, new();

    /// <summary>
    ///     Get single element from collection stored in single key
    /// </summary>
    /// <param name="entityName">Name of entity</param>
    /// <param name="key">Entity key</param>
    Task<TEntity?> GetFromSingleKeyAsync<TEntity>(string entityName, Guid key)
        where TEntity : class, IRepoEntity, new();

    /// <summary>
    ///     Drop hashset of given entity
    /// </summary>
    Task DropHashAsync<TEntity>();

    /// <summary>
    ///     Set items to cache hashset
    /// </summary>
    /// <param name="items">Items to store</param>
    Task StoreItemsAsHashAsync<TEntity>(IEnumerable<TEntity> items);

    /// <summary>
    ///     Drop single key with items of given entity
    /// </summary>
    /// <param name="entityName">Name of entity</param>
    Task DropSingleKeyAsync<TEntity>(string entityName);

    /// <summary>
    ///     Set items to single key
    /// </summary>
    /// <param name="entityName">Name of entity</param>
    /// <param name="items">Items to store</param>
    /// <param name="expiry">Expiration time</param>
    Task<bool> StoreItemsAsSingleKeyAsync<TEntity>(string entityName, IEnumerable<TEntity> items, TimeSpan expiry);
}