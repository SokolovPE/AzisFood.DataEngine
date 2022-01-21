#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AzisFood.DataEngine.Abstractions.Interfaces;
using MongoDB.Driver;

namespace AzisFood.DataEngine.Mongo;

/// <inheritdoc />
public class MongoDataAccess : IDataAccess
{
    private readonly IMongoDatabase _database;

    public MongoDataAccess(IMongoOptions mongoOptions)
    {
        var client = new MongoClient(mongoOptions.ConnectionString);
        _database = client.GetDatabase(mongoOptions.DatabaseName);
    }

    // For tests
    public MongoDataAccess(IMongoDatabase database)
    {
        _database = database;
        // Mock GetCollection of this database!
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TRepoEntity>> GetAllAsync<TRepoEntity>(CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity
    {
        return (await Collection<TRepoEntity>()
                .FindAsync(FilterDefinition<TRepoEntity>.Empty, cancellationToken: token))
            .ToEnumerable(token);
    }

    /// <inheritdoc />
    public async Task<TRepoEntity?> GetAsync<TRepoEntity>(Guid id, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity
    {
        return await (await Collection<TRepoEntity>().FindAsync(item => item.Id == id, null, token))
            .FirstOrDefaultAsync(token);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TRepoEntity>> GetAsync<TRepoEntity>(Expression<Func<TRepoEntity, bool>> filter,
        CancellationToken token = default) where TRepoEntity : class, IRepoEntity
    {
        return await (await Collection<TRepoEntity>().FindAsync(filter, cancellationToken: token))
            .ToListAsync(token);
    }

    /// <inheritdoc />
    public async Task<TRepoEntity> CreateAsync<TRepoEntity>(TRepoEntity item, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity
    {
        item.Id = Guid.NewGuid();
        await Collection<TRepoEntity>().InsertOneAsync(item, cancellationToken: token);
        return item;
    }

    /// <inheritdoc />
    public async Task UpdateAsync<TRepoEntity>(Guid id, TRepoEntity itemIn, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity
    {
        itemIn.Id = id;
        await Collection<TRepoEntity>().ReplaceOneAsync(item => item.Id == id, itemIn, cancellationToken: token);
    }

    /// <inheritdoc />
    public async Task RemoveAsync<TRepoEntity>(TRepoEntity itemIn, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity
    {
        await Collection<TRepoEntity>().DeleteOneAsync(item => item.Id == itemIn.Id, token);
    }

    /// <inheritdoc />
    public async Task RemoveAsync<TRepoEntity>(Guid id, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity
    {
        await Collection<TRepoEntity>().DeleteOneAsync(item => item.Id == id, token);
    }

    /// <inheritdoc />
    public async Task RemoveAsync<TRepoEntity>(Expression<Func<TRepoEntity, bool>> filter,
        CancellationToken token = default) where TRepoEntity : class, IRepoEntity
    {
        await Collection<TRepoEntity>().DeleteManyAsync(filter, token);
    }

    /// <inheritdoc />
    public async Task RemoveManyAsync<TRepoEntity>(Guid[] ids, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity
    {
        await Collection<TRepoEntity>().DeleteManyAsync(item => ((IList) ids).Contains(item.Id), token);
    }

    private IMongoCollection<TRepoEntity> Collection<TRepoEntity>()
    {
        return _database.GetCollection<TRepoEntity>(typeof(TRepoEntity).Name);
    }
}