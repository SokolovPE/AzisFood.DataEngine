#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AzisFood.DataEngine.Abstractions.Interfaces;
using AzisFood.DataEngine.Core;
using AzisFood.DataEngine.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace AzisFood.DataEngine.Postgres;

/// <inheritdoc />
public class PgDataAccess<TRepoEntity> : IDataAccess<TRepoEntity> 
    where TRepoEntity: PgRepoEntity
{
    private readonly DbContext _context;
    private readonly DbSet<TRepoEntity> _dbSet;
    public PgDataAccess(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<TRepoEntity>();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TRepoEntity>> GetAllAsync(CancellationToken token = default) =>
        await _dbSet.AsNoTracking().ToListAsync(token);

    /// <inheritdoc />
    public async Task<TRepoEntity?> GetAsync(Guid id, CancellationToken token = default) =>
        await _dbSet.FindAsync(id, token);

    /// <inheritdoc />
    public async Task<IEnumerable<TRepoEntity>> GetAsync(Expression<Func<TRepoEntity, bool>> filter,
        CancellationToken token = default) => await _dbSet.AsQueryable().Where(filter).ToListAsync(token);


    /// <inheritdoc />
    public async Task<TRepoEntity> CreateAsync(TRepoEntity item, CancellationToken token = default)
    {
        _dbSet.Add(item);
        await _context.SaveChangesAsync(token);
        return item;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Guid id, TRepoEntity itemIn, CancellationToken token = default)
    {
        itemIn.Id = id;
        _context.Entry(itemIn).State = EntityState.Modified;
        await _context.SaveChangesAsync(token);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(TRepoEntity itemIn, CancellationToken token = default)
    {
        _dbSet.Remove(itemIn);
        await _context.SaveChangesAsync(token);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(Guid id, CancellationToken token = default)
    {
        var entity = await GetAsync(id, token);
        if (entity != null)
        {
            await RemoveAsync(entity, token);
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(Expression<Func<TRepoEntity, bool>> filter, CancellationToken token = default)
    {
        var entities = await GetAsync(filter, token);
        _dbSet.RemoveRange(entities);
        await _context.SaveChangesAsync(token);
    }

    /// <inheritdoc />
    public async Task RemoveManyAsync(Guid[] ids, CancellationToken token = default)
    {
        // Process by small chunks to avoid SQL instruction length overflow
        await ids.ChunkedProcessAsync(50, async (guids) =>
        {
            await RemoveAsync(entity => guids.Contains(entity.Id), token);
        });
    }
}