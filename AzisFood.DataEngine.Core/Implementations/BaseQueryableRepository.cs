using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AzisFood.DataEngine.Abstractions.Interfaces;
using AzisFood.DataEngine.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace AzisFood.DataEngine.Core.Implementations;

public sealed class BaseQueryableRepository<TRepoEntity> : IBaseQueryableRepository<TRepoEntity>
    where TRepoEntity : class, IRepoEntity, new()
{
    private readonly IEnumerable<IDataAccess> _dataAccesses;
    private readonly Dictionary<Type, IDataAccess> _entityDataAccesses;
    private readonly ILogger<BaseQueryableRepository<TRepoEntity>> _logger;

    public BaseQueryableRepository(ILogger<BaseQueryableRepository<TRepoEntity>> logger, IEnumerable<IDataAccess> dataAccesses)
    {
        _logger = logger;
        _dataAccesses = dataAccesses;

        _entityDataAccesses = new Dictionary<Type, IDataAccess>();

        // Fill constants
        RepoEntityName = typeof(TRepoEntity).Name;
    }

    public string RepoEntityName { get; init; }

    public async Task<IEnumerable<TRepoEntity>> GetAsync(bool track = false, CancellationToken token = default)
    {
        _logger.LogInformation("Requested all {RepoEntityName} items", RepoEntityName);
        try
        {
            token.ThrowIfCancellationRequested();
            var repoEntities = await DataAccess().GetAllAsync<TRepoEntity>(track, token);
            _logger.LogInformation(
                "Request of all {RepoEntityName} items succeeded", RepoEntityName);
            return repoEntities;
        }
        catch (OperationCanceledException)
        {
            // Throw cancelled operation, do not catch
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "There was an error during attempt to return all {RepoEntityName} items", RepoEntityName);
            return default;
        }
    }

    /// <inheritdoc />
    public IQueryable<TRepoEntity> GetQueryable(bool track = false)
    {
        _logger.LogInformation("Requested all {RepoEntityName} items as queryable", RepoEntityName);
        try
        {
            var repoQueryable = DataAccess().GetAllQueryable<TRepoEntity>(track);
            _logger.LogInformation(
                "Request of all {RepoEntityName} items as queryable succeeded", RepoEntityName);
            return repoQueryable;
        }
        catch (OperationCanceledException)
        {
            // Throw cancelled operation, do not catch
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "There was an error during attempt to return all {RepoEntityName} items as queryable",
                RepoEntityName);
            return default;
        }
    }

    public async Task<TRepoEntity> GetAsync(Guid id, bool track = false, CancellationToken token = default)
    {
        _logger.LogInformation("Requested {RepoEntityName} with id: {Id}", RepoEntityName, id);
        try
        {
            token.ThrowIfCancellationRequested();
            var repoEntity = await DataAccess().GetAsync<TRepoEntity>(id, track, token);
            if (repoEntity == null) throw new InvalidOperationException($"{RepoEntityName} with id {id} was not found");
            _logger.LogInformation("Request of {RepoEntityName} with id: {Id} succeeded", RepoEntityName, id);
            return repoEntity;
        }
        catch (OperationCanceledException)
        {
            // Throw cancelled operation, do not catch
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "There was an error during attempt to return {RepoEntityName} item", RepoEntityName);
            return default;
        }
    }

    public async Task<IEnumerable<TRepoEntity>> GetAsync(Expression<Func<TRepoEntity, bool>> filter,
        bool track = false, CancellationToken token = default)
    {
        _logger.LogInformation("Requested filtered {RepoEntityName} items, filter: {@Filter}", RepoEntityName, filter);
        try
        {
            token.ThrowIfCancellationRequested();

            var repoEntities = await DataAccess().GetAsync(filter, track, token);
            _logger.LogInformation(
                "Request of filtered {RepoEntityName} items succeeded, filter: {@Filter}", RepoEntityName, filter);
            return repoEntities;
        }
        catch (OperationCanceledException)
        {
            // Throw cancelled operation, do not catch
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "There was an error during attempt to return {RepoEntityName} items with filter {@Filter}",
                RepoEntityName, filter);
            return default;
        }
    }

    /// <inheritdoc />
    public IQueryable<TRepoEntity> GetQueryable(Expression<Func<TRepoEntity, bool>> filter, bool track = false)
    {
        _logger.LogInformation("Requested filtered {RepoEntityName} items as queryable, filter: {@Filter}",
            RepoEntityName, filter);
        try
        {
            var repoEntities = DataAccess().GetQueryable(filter, track);
            _logger.LogInformation(
                "Request of filtered {RepoEntityName} items as queryable succeeded, filter: {@Filter}", RepoEntityName,
                filter);
            return repoEntities;
        }
        catch (OperationCanceledException)
        {
            // Throw cancelled operation, do not catch
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "There was an error during attempt to return {RepoEntityName} items as queryable with filter {@Filter}",
                RepoEntityName, filter);
            return default;
        }
    }

    public async Task<TRepoEntity> CreateAsync(TRepoEntity item, CancellationToken token = default)
    {
        _logger.LogInformation("Requested creation of {RepoEntityName}: {@Item}", RepoEntityName, item);
        try
        {
            token.ThrowIfCancellationRequested();
            var created = await DataAccess().CreateAsync(item, token);
            _logger.LogInformation("Requested creation of {RepoEntityName} succeeded", RepoEntityName);
            return created;
        }
        catch (OperationCanceledException)
        {
            // Throw cancelled operation, do not catch
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "There was an error during attempt to create {RepoEntityName}", RepoEntityName);
            return default;
        }
    }

    public async Task UpdateAsync(Guid id, TRepoEntity itemIn, CancellationToken token = default)
    {
        _logger.LogInformation(
            "Requested update of {RepoEntityName} with id {Id} with new value: {@Item}", RepoEntityName, id, itemIn);
        try
        {
            await DataAccess().UpdateAsync(id, itemIn, token);
            _logger.LogInformation("Requested update of {RepoEntityName} succeeded", RepoEntityName);
        }
        catch (OperationCanceledException)
        {
            // Throw cancelled operation, do not catch
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "There was an error during attempt to update {RepoEntityName}", RepoEntityName);
        }
    }

    public async Task RemoveAsync(TRepoEntity itemIn, CancellationToken token = default)
    {
        _logger.LogInformation("Requested delete of {RepoEntityName}: {@Item}", RepoEntityName, itemIn);
        try
        {
            await DataAccess().RemoveAsync(itemIn, token);
            _logger.LogInformation("Requested delete of {RepoEntityName} succeeded", RepoEntityName);
        }
        catch (OperationCanceledException)
        {
            // Throw cancelled operation, do not catch
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "There was an error during attempt to delete {RepoEntityName}", RepoEntityName);
        }
    }

    public async Task RemoveAsync(Guid id, CancellationToken token = default)
    {
        _logger.LogInformation("Requested delete of {RepoEntityName} with id {Id}", RepoEntityName, id);
        try
        {
            await DataAccess().RemoveAsync<TRepoEntity>(id, token);
            _logger.LogInformation("Requested delete of {RepoEntityName} succeeded", RepoEntityName);
        }
        catch (OperationCanceledException)
        {
            // Throw cancelled operation, do not catch
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "There was an error during attempt to delete {RepoEntityName}", RepoEntityName);
        }
    }

    public async Task RemoveAsync(Expression<Func<TRepoEntity, bool>> filter, CancellationToken token = default)
    {
        _logger.LogInformation("Requested delete of {RepoEntityName} with filter {@Filter}", RepoEntityName, filter);
        try
        {
            await DataAccess().RemoveAsync(filter, token);
            _logger.LogInformation("Requested delete of {RepoEntityName} with filter {@Filter} succeeded",
                RepoEntityName, filter);
        }
        catch (OperationCanceledException)
        {
            // Throw cancelled operation, do not catch
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "There was an error during attempt to delete {RepoEntityName}", RepoEntityName);
        }
    }

    public async Task RemoveManyAsync(Guid[] ids, CancellationToken token = default)
    {
        _logger.LogInformation(
            "Requested delete of multiple {RepoEntityName} with ids {Ids}", RepoEntityName, (object) ids);
        try
        {
            await DataAccess().RemoveManyAsync<TRepoEntity>(ids, token);
            _logger.LogInformation("Requested delete of multiple {RepoEntityName} succeeded", RepoEntityName);
        }
        catch (OperationCanceledException)
        {
            // Throw cancelled operation, do not catch
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "There was an error during attempt to delete multiple {RepoEntityName}",
                RepoEntityName);
        }
    }

    /// <summary>
    ///     Get data access for given type
    /// </summary>
    /// <typeparam name="TRepoEntity">Entity type</typeparam>
    /// <exception cref="ArgumentException">If entity contains no required attribute exception will be thrown</exception>
    private IDataAccess DataAccess()
    {
        var type = typeof(TRepoEntity);

        // First - check dictionary to avoid reflection
        if (_entityDataAccesses.ContainsKey(type)) return _entityDataAccesses[type];

        // If info is not presented in dictionary scan type and attribute
        var fullName = type.FullName;

        var parentType = type.BaseType;
        if (parentType == null)
            throw new ArgumentException(
                $"Entity {fullName} has no base class. Entity must be based on {nameof(IRepoEntity)} implementation");

        if (Attribute.GetCustomAttribute(parentType, typeof(SupportedBy)) is not SupportedBy attribute)
            throw new ArgumentException(
                $"Entity {fullName} has no {nameof(SupportedBy)} attribute. Entity is not supported");

        // Now let's find out which context is suitable
        try
        {
            var access = _dataAccesses.First(access => access.DbType == attribute.Type.ToString());
            _entityDataAccesses.Add(type, access);
            return access;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"There's no data access suitable for {fullName}. Verify that {attribute.Type} data access was registered",
                e);
        }
    }
}