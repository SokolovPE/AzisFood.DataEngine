#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AzisFood.DataEngine.Abstractions.Interfaces;
using AzisFood.DataEngine.Core;
using Microsoft.EntityFrameworkCore;

namespace AzisFood.DataEngine.Postgres;

/// <inheritdoc />
public class PgDataAccess : IDataAccess
{
    private readonly DbContext _context;

    public PgDataAccess(DbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TRepoEntity>> GetAllAsync<TRepoEntity>(CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity
    {
        return await Collection<TRepoEntity>().AsNoTracking().ToListAsync(token);
    }

    /// <inheritdoc />
    public async Task<TRepoEntity?> GetAsync<TRepoEntity>(Guid id, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity
    {
        return await Collection<TRepoEntity>().FindAsync(id, token);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TRepoEntity>> GetAsync<TRepoEntity>(Expression<Func<TRepoEntity, bool>> filter,
        CancellationToken token = default) where TRepoEntity : class, IRepoEntity
    {
        return await Collection<TRepoEntity>().AsQueryable().Where(filter).ToListAsync(token);
    }


    /// <inheritdoc />
    public async Task<TRepoEntity> CreateAsync<TRepoEntity>(TRepoEntity item, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity
    {
        Collection<TRepoEntity>().Add(item);
        await _context.SaveChangesAsync(token);
        return item;
    }

    /// <inheritdoc />
    public async Task UpdateAsync<TRepoEntity>(Guid id, TRepoEntity itemIn, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity
    {
        itemIn.Id = id;
        _context.Entry(itemIn).State = EntityState.Modified;
        await _context.SaveChangesAsync(token);
    }

    /// <inheritdoc />
    public async Task RemoveAsync<TRepoEntity>(TRepoEntity itemIn, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity
    {
        Collection<TRepoEntity>().Remove(itemIn);
        await _context.SaveChangesAsync(token);
    }

    /// <inheritdoc />
    public async Task RemoveAsync<TRepoEntity>(Guid id, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity
    {
        var entity = await GetAsync<TRepoEntity>(id, token);
        if (entity != null) await RemoveAsync(entity, token);
    }

    /// <inheritdoc />
    public async Task RemoveAsync<TRepoEntity>(Expression<Func<TRepoEntity, bool>> filter,
        CancellationToken token = default) where TRepoEntity : class, IRepoEntity
    {
        var entities = await GetAsync(filter, token);
        Collection<TRepoEntity>().RemoveRange(entities);
        await _context.SaveChangesAsync(token);
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
        return _context.Set<TRepoEntity>();
    }
}