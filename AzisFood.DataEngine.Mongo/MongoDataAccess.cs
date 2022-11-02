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
using AzisFood.DataEngine.Mongo.Models;
using MongoDB.Driver;

namespace AzisFood.DataEngine.Mongo;

/// <inheritdoc />
public class MongoDataAccess : IDataAccess
{
    private readonly MongoConfiguration _configuration = null!;
    private readonly IEnumerable<IMongoDatabase> _databases = null!;
    private readonly Dictionary<Type, IMongoDatabase> _entityDatabases;

    public MongoDataAccess(IEnumerable<IMongoDatabase> databases, MongoConfiguration configuration)
    {
        _databases = databases;
        _configuration = configuration;
        _entityDatabases = new Dictionary<Type, IMongoDatabase>();
    }

    // For tests
    public MongoDataAccess(IMongoDatabase database, Type type)
    {
        _entityDatabases = new Dictionary<Type, IMongoDatabase> {{type, database}};
    }

    /// <inheritdoc />
    public string DbType { get; set; } = DatabaseType.Mongo.ToString();

    /// <inheritdoc />
    public async Task<IEnumerable<TRepoEntity>> GetAllAsync<TRepoEntity>(bool track = false, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity
    {
        return (await Collection<TRepoEntity>()
                .FindAsync(FilterDefinition<TRepoEntity>.Empty, cancellationToken: token))
            .ToEnumerable(token);
    }

    /// <summary>
    ///     Not implemented
    /// </summary>
    public Task<IEnumerable<TProject>> GetAllAsync<TRepoEntity, TProject>(Func<IQueryable<TRepoEntity>, IQueryable<TProject>> projector, bool track = false, CancellationToken token = default) where TRepoEntity : class, IRepoEntity
    {
        throw new NotImplementedException();
    }


    /// <summary>
    ///     Not implemented
    /// </summary>
    public IQueryable<TRepoEntity> GetAllQueryable<TRepoEntity>(bool track = false) where TRepoEntity : class, IRepoEntity
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Not implemented
    /// </summary>
    public IQueryable<TProject> GetAllQueryable<TRepoEntity, TProject>(
        Func<IQueryable<TRepoEntity>, IQueryable<TProject>> projector, bool track = false)
        where TRepoEntity : class, IRepoEntity
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<TRepoEntity?> GetAsync<TRepoEntity>(Guid id, bool track = false, CancellationToken token = default)
        where TRepoEntity : class, IRepoEntity
    {
        return await (await Collection<TRepoEntity>().FindAsync(item => item.Id == id, null, token))
            .FirstOrDefaultAsync(token);
    }

    /// <summary>
    ///     Not implemented
    /// </summary>
    public Task<TProject?> GetAsync<TRepoEntity, TProject>(Guid id,
        Func<IQueryable<TRepoEntity>, IQueryable<TProject>> projector, bool track = false,
        CancellationToken token = default) where TRepoEntity : class, IRepoEntity
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TRepoEntity>> GetAsync<TRepoEntity>(Expression<Func<TRepoEntity, bool>>? filter = null,
        bool track = false, CancellationToken token = default) where TRepoEntity : class, IRepoEntity
    {
        return await (await Collection<TRepoEntity>().FindAsync(filter, cancellationToken: token))
            .ToListAsync(token);
    }

    /// <summary>
    ///     Not implemented
    /// </summary>
    public Task<IEnumerable<TProject>> GetAsync<TRepoEntity, TProject>(
        Func<IQueryable<TRepoEntity>, IQueryable<TProject>> projector,
        Expression<Func<TRepoEntity, bool>>? filter = null, bool track = false,
        CancellationToken token = default) where TRepoEntity : class, IRepoEntity
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Not implemented
    /// </summary>
    public IQueryable<TRepoEntity> GetQueryable<TRepoEntity>(Expression<Func<TRepoEntity, bool>>? filter = null,
        bool track = false)
        where TRepoEntity : class, IRepoEntity
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Not implemented
    /// </summary>
    public IQueryable<TProject> GetQueryable<TRepoEntity, TProject>(
        Func<IQueryable<TRepoEntity>, IQueryable<TProject>> projector,
        Expression<Func<TRepoEntity, bool>>? filter = null, bool track = false) where TRepoEntity : class, IRepoEntity
    {
        throw new NotImplementedException();
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
    public async Task RemoveAsync<TRepoEntity>(Expression<Func<TRepoEntity, bool>>? filter = null,
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

    /// <inheritdoc />
    public async Task<bool> ExistsAsync<TRepoEntity>(Guid id, CancellationToken token = default) where TRepoEntity : class, IRepoEntity
    {
        var itemExists =
            await (await Collection<TRepoEntity>().FindAsync(filter: item => item.Id == id, cancellationToken: token))
                .AnyAsync(token);
        return itemExists;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync<TRepoEntity>(Expression<Func<TRepoEntity, bool>> filter, CancellationToken token = default) where TRepoEntity : class, IRepoEntity
    {
        var itemExists = await (await Collection<TRepoEntity>().FindAsync(filter: filter, cancellationToken: token)).AnyAsync(token);
        return itemExists;
    }

    private IMongoCollection<TRepoEntity> Collection<TRepoEntity>() where TRepoEntity : class, IRepoEntity
    {
        return Database<TRepoEntity>().GetCollection<TRepoEntity>(typeof(TRepoEntity).Name);
    }

    /// <summary>
    ///     Get entity client for given type
    /// </summary>
    /// <typeparam name="TRepoEntity">Entity type</typeparam>
    /// <exception cref="ArgumentException">If entity contains no required attribute exception will be thrown</exception>
    private IMongoDatabase Database<TRepoEntity>() where TRepoEntity : class, IRepoEntity
    {
        var type = typeof(TRepoEntity);

        // First - check dictionary to avoid reflection
        if (_entityDatabases.ContainsKey(type)) return _entityDatabases[type];

        // If info is not presented in dictionary scan type and attribute
        var fullName = type.FullName;

        if (Attribute.GetCustomAttribute(type, typeof(ConnectionAlias)) is not ConnectionAlias attribute)
            throw new ArgumentException(
                $"Entity {fullName} has no {nameof(ConnectionAlias)} attribute. Entity is not supported");

        // Now let's find out which context is suitable
        try
        {
            var connect = _configuration.Connections.First(con => con.Alias == attribute.Alias);
            var database =
                _databases.First(db => db.DatabaseNamespace.DatabaseName == connect.GetMongoUrl.DatabaseName);
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