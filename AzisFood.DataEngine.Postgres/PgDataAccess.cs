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
using AzisFood.DataEngine.Postgres.Models;
using Microsoft.EntityFrameworkCore;

namespace AzisFood.DataEngine.Postgres;

/// <inheritdoc />
public class PgDataAccess : IDataAccess
{
    private readonly IEnumerable<DbContext> _contexts;
    private readonly Dictionary<Type, DbContext> _entityContexts;

    public PgDataAccess(IEnumerable<DbContext> contexts)
    {
        _contexts = contexts;
        _entityContexts = new Dictionary<Type, DbContext>();
    }

    /// <inheritdoc />
    public string DbType { get; set; } = DatabaseType.Postgres.ToString();

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
        var entity = await GetAsync<TRepoEntity>(id, token);
        if (entity != null) await RemoveAsync(entity, token);
    }

    /// <inheritdoc />
    public async Task RemoveAsync<TRepoEntity>(Expression<Func<TRepoEntity, bool>> filter,
        CancellationToken token = default) where TRepoEntity : class, IRepoEntity
    {
        var entities = await GetAsync(filter, token);
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
    /// <exception cref="ArgumentException">If entity contains no required attribute exception will be thrown</exception>
    private DbContext Context<TRepoEntity>() where TRepoEntity : class, IRepoEntity
    {
        var type = typeof(TRepoEntity);

        // First - check dictionary to avoid reflection
        if (_entityContexts.ContainsKey(type)) return _entityContexts[type];

        // If info is not presented in dictionary scan type and attribute
        var fullName = type.FullName;

        var contextType = type.BaseType?.GetGenericArguments().FirstOrDefault();
        if (contextType == default)
            throw new ArgumentException(
                $"Entity {fullName} has no specified DbContext. " +
                $"Specify it via generic parameter of {nameof(PgRepoEntity<DbContext>)}");

        // Now let's find out which context is suitable
        try
        {
            var context = _contexts.First(ctx => ctx.GetType().Name == contextType.Name);
            _entityContexts.Add(type, context);
            return context;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"There's no context suitable for {fullName}. Verify that at least one registered context " +
                "has a set for this entity", e);
        }
    }
}