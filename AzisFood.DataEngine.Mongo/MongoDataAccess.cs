#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AzisFood.DataEngine.Abstractions.Interfaces;
using AzisFood.DataEngine.Core;
using AzisFood.DataEngine.Core.Attributes;
using AzisFood.DataEngine.Mongo.Implementations;
using MongoDB.Driver;

namespace AzisFood.DataEngine.Mongo;

/// <inheritdoc />
public class MongoDataAccess : IDataAccess
{
    private readonly MongoOptions _options;
    private readonly IEnumerable<IMongoDatabase> _databases;
    private readonly Dictionary<Type, IMongoDatabase> _entityDatabases;

    public MongoDataAccess(IEnumerable<IMongoDatabase> databases, MongoOptions options)
    {
        _databases = databases;
        _options = options;
        _entityDatabases = new Dictionary<Type, IMongoDatabase>();
    }

    // For tests
    public MongoDataAccess(IMongoDatabase database, Type type)
    {
        _entityDatabases = new Dictionary<Type, IMongoDatabase>();
        _entityDatabases.Add(type, database);
    }

    /// <inheritdoc />
    public string DbType { get; set; } = DatabaseType.Mongo.ToString();

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

    private IMongoCollection<TRepoEntity> Collection<TRepoEntity>() where TRepoEntity : class, IRepoEntity
    {
        return Database<TRepoEntity>().GetCollection<TRepoEntity>(typeof(TRepoEntity).Name);
    }

    /// <summary>
    /// Get entity client for given type
    /// </summary>
    /// <typeparam name="TRepoEntity">Entity type</typeparam>
    /// <exception cref="ArgumentException">If entity contains no required attribute exception will be thrown</exception>
    private IMongoDatabase Database<TRepoEntity>() where TRepoEntity : class, IRepoEntity
    {
        var type = typeof(TRepoEntity);

        // First - check dictionary to avoid reflection
        if (_entityDatabases.ContainsKey(type))
        {
            return _entityDatabases[type];
        }

        // If info is not presented in dictionary scan type and attribute
        var fullName = type.FullName;

        var attribute = Attribute.GetCustomAttribute(type, typeof(UseContext)) as UseContext;
        if (attribute == null)
        {
            throw new ArgumentException(
                $"Entity {fullName} has no {nameof(UseContext)} attribute. Entity is not supported");
        }
        
        // Now let's find out which context is suitable
        try
        {
            var connect = _options.Connections.First(con => con.ConnectionName == attribute.ContextName);
            var database = _databases.First(db => db.DatabaseNamespace.DatabaseName == connect.Database);
            _entityDatabases.Add(type, database);
            return database;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"There's no connection suitable for {fullName}. Verify that at least one connect is registered for this entity",
                e);
        }
    }
}