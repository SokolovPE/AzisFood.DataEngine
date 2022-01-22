using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AzisFood.DataEngine.Abstractions.Interfaces;
using AzisFood.DataEngine.Core.Attributes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzisFood.DataEngine.Core.Implementations;

public sealed class BaseRepository<TRepoEntity> : IBaseRepository<TRepoEntity>
    where TRepoEntity : class, IRepoEntity, new()
{
    private readonly IEnumerable<IDataAccess> _dataAccesses;
    private readonly Dictionary<Type, IDataAccess> _entityDataAccesses;
    private readonly ILogger<BaseRepository<TRepoEntity>> _logger;

    public BaseRepository(ILogger<BaseRepository<TRepoEntity>> logger, IEnumerable<IDataAccess> dataAccesses)
    {
        _logger = logger;
        _dataAccesses = dataAccesses;

        _entityDataAccesses = new Dictionary<Type, IDataAccess>();

        // Fill constants
        RepoEntityName = typeof(TRepoEntity).Name;
    }

    public string RepoEntityName { get; init; }

    public async Task<IEnumerable<TRepoEntity>> GetAsync(CancellationToken token = default)
    {
        _logger.LogInformation($"Requested all {RepoEntityName} items");
        try
        {
            token.ThrowIfCancellationRequested();
            var repoEntities = await DataAccess().GetAllAsync<TRepoEntity>(token);
            _logger.LogInformation(
                $"Request of all {RepoEntityName} items succeeded");
            return repoEntities;
        }
        catch (OperationCanceledException)
        {
            // Throw cancelled operation, do not catch
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"There was an error during attempt to return all {RepoEntityName} items");
            return default;
        }
    }

    public async Task<TRepoEntity> GetAsync(Guid id, CancellationToken token = default)
    {
        _logger.LogInformation($"Requested {RepoEntityName} with id: {id}");
        try
        {
            token.ThrowIfCancellationRequested();
            var repoEntity = await DataAccess().GetAsync<TRepoEntity>(id, token);
            if (repoEntity == null) throw new InvalidOperationException($"{RepoEntityName} with id {id} was not found");
            _logger.LogInformation($"Request of {RepoEntityName} with id: {id} succeeded");
            return repoEntity;
        }
        catch (OperationCanceledException)
        {
            // Throw cancelled operation, do not catch
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"There was an error during attempt to return {RepoEntityName} item");
            return default;
        }
    }

    public async Task<IEnumerable<TRepoEntity>> GetAsync(Expression<Func<TRepoEntity, bool>> filter,
        CancellationToken token = default)
    {
        _logger.LogInformation($"Requested filtered {RepoEntityName} items, filter: {filter}");
        try
        {
            token.ThrowIfCancellationRequested();

            var repoEntities = await DataAccess().GetAsync(filter, token);
            _logger.LogInformation(
                $"Request of filtered {RepoEntityName} items succeeded, filter: {filter}");
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
                $"There was an error during attempt to return {RepoEntityName} items with filter {filter}");
            return default;
        }
    }

    public async Task<TRepoEntity> CreateAsync(TRepoEntity item, CancellationToken token = default)
    {
        _logger.LogInformation($"Requested creation of {RepoEntityName}: {JsonConvert.SerializeObject(item)}");
        try
        {
            token.ThrowIfCancellationRequested();
            await DataAccess().CreateAsync(item, token);
            _logger.LogInformation($"Requested creation of {RepoEntityName} succeeded");
            return item;
        }
        catch (OperationCanceledException)
        {
            // Throw cancelled operation, do not catch
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"There was an error during attempt to create {RepoEntityName}");
            return default;
        }
    }

    public async Task UpdateAsync(Guid id, TRepoEntity itemIn, CancellationToken token = default)
    {
        _logger.LogInformation(
            $"Requested update of {RepoEntityName} with id {id} with new value: {JsonConvert.SerializeObject(itemIn)}");
        try
        {
            await DataAccess().UpdateAsync(id, itemIn, token);
            _logger.LogInformation($"Requested update of {RepoEntityName} succeeded");
        }
        catch (OperationCanceledException)
        {
            // Throw cancelled operation, do not catch
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"There was an error during attempt to update {RepoEntityName}");
        }
    }

    public async Task RemoveAsync(TRepoEntity itemIn, CancellationToken token = default)
    {
        _logger.LogInformation($"Requested delete of {RepoEntityName}: {JsonConvert.SerializeObject(itemIn)}");
        try
        {
            await DataAccess().RemoveAsync(itemIn, token);
            _logger.LogInformation($"Requested delete of {RepoEntityName} succeeded");
        }
        catch (OperationCanceledException)
        {
            // Throw cancelled operation, do not catch
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"There was an error during attempt to delete {RepoEntityName}");
        }
    }

    public async Task RemoveAsync(Guid id, CancellationToken token = default)
    {
        _logger.LogInformation($"Requested delete of {RepoEntityName} with id {id}");
        try
        {
            await DataAccess().RemoveAsync<TRepoEntity>(id, token);
            _logger.LogInformation($"Requested delete of {RepoEntityName} succeeded");
        }
        catch (OperationCanceledException)
        {
            // Throw cancelled operation, do not catch
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"There was an error during attempt to delete {RepoEntityName}");
        }
    }

    public async Task RemoveAsync(Expression<Func<TRepoEntity, bool>> filter, CancellationToken token = default)
    {
        _logger.LogInformation($"Requested delete of {RepoEntityName} with filter {filter}");
        try
        {
            await DataAccess().RemoveAsync(filter, token);
            _logger.LogInformation($"Requested delete of {RepoEntityName} with filter {filter} succeeded");
        }
        catch (OperationCanceledException)
        {
            // Throw cancelled operation, do not catch
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"There was an error during attempt to delete {RepoEntityName}");
        }
    }

    public async Task RemoveManyAsync(Guid[] ids, CancellationToken token = default)
    {
        _logger.LogInformation(
            $"Requested delete of multiple {RepoEntityName} with ids {JsonConvert.SerializeObject(ids)}");
        try
        {
            await DataAccess().RemoveManyAsync<TRepoEntity>(ids, token);
            _logger.LogInformation($"Requested delete of multiple {RepoEntityName} succeeded");
        }
        catch (OperationCanceledException)
        {
            // Throw cancelled operation, do not catch
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"There was an error during attempt to delete multiple {RepoEntityName}");
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

        var attribute = Attribute.GetCustomAttribute(type, typeof(SupportedBy)) as SupportedBy;
        if (attribute == null)
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