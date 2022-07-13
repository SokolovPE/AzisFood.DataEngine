#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AzisFood.DataEngine.Abstractions.Interfaces;
using AzisFood.DataEngine.Core;
using AzisFood.DataEngine.Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace AzisFood.DataEngine.Postgres;

/// <inheritdoc />
public class PgDataAccess : IDataAccess
{
    // private readonly Dictionary<Type, DbContext> _entityContexts;
    private readonly IServiceProvider _serviceProvider;
    public PgDataAccess(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        // _entityContexts = new Dictionary<Type, DbContext>();
    }

    /// <inheritdoc />
    public string DbType { get; set; } = DatabaseType.Postgres.ToString();

    /// <inheritdoc />
    public async Task<IEnumerable<TRepoEntity>> GetAllAsync<TRepoEntity>(bool track = false, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity => await GetAllQueryable<TRepoEntity>(track).ToListAsync(token);

    /// <inheritdoc />
    public IQueryable<TRepoEntity> GetAllQueryable<TRepoEntity>(bool track = false) where TRepoEntity : class, IRepoEntity
    {
        var query = Collection<TRepoEntity>().AsQueryable();
        if(!track)
            query = query.AsNoTracking();
        return query;
    }

    /// <inheritdoc />
    public async Task<TRepoEntity?> GetAsync<TRepoEntity>(Guid id, bool track = false, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity
    {
        var entity = await Collection<TRepoEntity>().FindAsync(new object?[] { id }, cancellationToken: token);
        if (!track && entity != null)
            Context<TRepoEntity>().Entry(entity).State = EntityState.Detached;
        return entity;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TRepoEntity>> GetAsync<TRepoEntity>(Expression<Func<TRepoEntity, bool>> filter,
        bool track = false, CancellationToken token = default) where TRepoEntity : class, IRepoEntity
    {
        var query = Collection<TRepoEntity>().AsQueryable();
        if (!track)
            query = query.AsNoTracking();
        return await query.Where(filter).ToListAsync(token);
    }

    /// <inheritdoc />
    public IQueryable<TRepoEntity> GetQueryable<TRepoEntity>(Expression<Func<TRepoEntity, bool>> filter, bool track = false)
        where TRepoEntity : class, IRepoEntity
    {
        var query = Collection<TRepoEntity>().AsQueryable();
        if (!track)
            query = query.AsNoTracking();
        return query.Where(filter);
    }

    /// <inheritdoc />
    public async Task<TRepoEntity> CreateAsync<TRepoEntity>(TRepoEntity item, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity
    {
        Collection<TRepoEntity>().Add(item);
        await Context<TRepoEntity>().SaveChangesAsync(token);
        return item;
    }

    /// <inheritdoc />
    public async Task UpdateAsync<TRepoEntity>(Guid id, TRepoEntity itemIn, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity
    {
        itemIn.Id = id;
        Context<TRepoEntity>().Entry(itemIn).State = EntityState.Modified;
        await Context<TRepoEntity>().SaveChangesAsync(token);
    }

    /// <inheritdoc />
    public async Task RemoveAsync<TRepoEntity>(TRepoEntity itemIn, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity
    {
        Collection<TRepoEntity>().Remove(itemIn);
        await Context<TRepoEntity>().SaveChangesAsync(token);
    }

    /// <inheritdoc />
    public async Task RemoveAsync<TRepoEntity>(Guid id, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity
    {
        var entity = await GetAsync<TRepoEntity>(id, false, token);
        if (entity != null) await RemoveAsync(entity, token);
    }

    /// <inheritdoc />
    public async Task RemoveAsync<TRepoEntity>(Expression<Func<TRepoEntity, bool>> filter,
        CancellationToken token = default) where TRepoEntity : class, IRepoEntity
    {
        var entities = await GetAsync(filter, false, token);
        Collection<TRepoEntity>().RemoveRange(entities);
        await Context<TRepoEntity>().SaveChangesAsync(token);
    }

    /// <inheritdoc />
    public async Task RemoveManyAsync<TRepoEntity>(Guid[] ids, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity
    {
        // Process by small chunks to avoid SQL instruction length overflow
        await ids.ChunkedProcessAsync(50,
            async guids => { await RemoveAsync<TRepoEntity>(entity => guids.Contains(entity.Id), token); });
    }

    private DbSet<TRepoEntity> Collection<TRepoEntity>() where TRepoEntity : class, IRepoEntity
    {
        return Context<TRepoEntity>().Set<TRepoEntity>();
    }

    /// <summary>
    ///     Get entity context for given type
    /// </summary>
    /// <typeparam name="TRepoEntity">Entity type</typeparam>
    private DbContext Context<TRepoEntity>() where TRepoEntity : class, IRepoEntity
    {
        try
        {
            return DbContextAccess.Create<TRepoEntity>(_serviceProvider);
        }
        catch (Exception e)
        {
            throw new Exception($"Cannot create suitable dbContext for type {typeof(TRepoEntity).Name}", e);
        }
    }
}