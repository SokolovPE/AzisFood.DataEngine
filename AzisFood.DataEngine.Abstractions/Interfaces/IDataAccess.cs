#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// <param name="track">Should entity be tracked</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Items collection</returns>
    Task<IEnumerable<TRepoEntity>> GetAllAsync<TRepoEntity>(bool track = false, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity;

    /// <summary>
    ///     Get all entities projected
    /// </summary>
    /// <param name="projector">Projector function</param>
    /// <param name="track">Should entity be tracked</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Items collection</returns>
    Task<IEnumerable<TProject>> GetAllAsync<TRepoEntity, TProject>(
        Func<IQueryable<TRepoEntity>, IQueryable<TProject>> projector, bool track = false,
        CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity;
    
    /// <summary>
    ///     Get all entities as queryable
    /// </summary>
    /// <returns>Items collection</returns>
    IQueryable<TRepoEntity> GetAllQueryable<TRepoEntity>(bool track = false)
        where TRepoEntity : class, IRepoEntity;

    /// <summary>
    ///     Get all entities as queryable
    /// </summary>
    /// <returns>Items collection</returns>
    IQueryable<TProject> GetAllQueryable<TRepoEntity, TProject>(
        Func<IQueryable<TRepoEntity>, IQueryable<TProject>> projector, bool track = false)
        where TRepoEntity : class, IRepoEntity;

    /// <summary>
    ///     Get single item
    /// </summary>
    /// <param name="id">Identifier of item</param>
    /// <param name="track">Should entity be tracked</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Requested item</returns>
    Task<TRepoEntity?> GetAsync<TRepoEntity>(Guid id, bool track = false, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity;

    /// <summary>
    ///     Get single item projected
    /// </summary>
    /// <param name="id">Identifier of item</param>
    /// <param name="projector">Projector function</param>
    /// <param name="track">Should entity be tracked</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Requested item</returns>
    Task<TProject?> GetAsync<TRepoEntity, TProject>(Guid id,
        Func<IQueryable<TRepoEntity>, IQueryable<TProject>> projector, bool track = false,
        CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity;

    /// <summary>
    ///     Get filtered items
    /// </summary>
    /// <param name="filter">Filter expression</param>
    /// <param name="track">Should entity be tracked</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Filtered items</returns>
    Task<IEnumerable<TRepoEntity>> GetAsync<TRepoEntity>(Expression<Func<TRepoEntity, bool>>? filter = null,
        bool track = false, CancellationToken token = default) where TRepoEntity : class, IRepoEntity;

    /// <summary>
    ///     Get filtered items projected
    /// </summary>
    /// <param name="filter">Filter expression</param>
    /// <param name="projector">Projector function</param>
    /// <param name="track">Should entity be tracked</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Filtered items</returns>
    Task<IEnumerable<TProject>> GetAsync<TRepoEntity, TProject>(
        Func<IQueryable<TRepoEntity>, IQueryable<TProject>> projector,
        Expression<Func<TRepoEntity, bool>>? filter = null, bool track = false,
        CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity;

    /// <summary>
    ///     Get filtered items
    /// </summary>
    /// <param name="filter">Filter expression</param>
    /// <param name="track">Should entity be tracked</param>
    /// <returns>Filtered items</returns>
    IQueryable<TRepoEntity> GetQueryable<TRepoEntity>(Expression<Func<TRepoEntity, bool>>? filter = null,
        bool track = false)
        where TRepoEntity : class, IRepoEntity;

    /// <summary>
    ///     Get filtered items projected
    /// </summary>
    /// <param name="filter">Filter expression</param>
    /// <param name="projector">Projector function</param>
    /// <param name="track">Should entity be tracked</param>
    /// <returns>Filtered items</returns>
    IQueryable<TProject> GetQueryable<TRepoEntity, TProject>(
        Func<IQueryable<TRepoEntity>, IQueryable<TProject>> projector,
        Expression<Func<TRepoEntity, bool>>? filter = null, bool track = false)
        where TRepoEntity : class, IRepoEntity;
    
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
    Task RemoveAsync<TRepoEntity>(Expression<Func<TRepoEntity, bool>>? filter = null, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity;

    /// <summary>
    ///     Remove items by ids
    /// </summary>
    /// <param name="ids">List of identifiers</param>
    /// <param name="token">Cancellation token</param>
    Task RemoveManyAsync<TRepoEntity>(Guid[] ids, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity;
}