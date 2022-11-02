using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AzisFood.DataEngine.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;
using OpenTracing;

namespace AzisFood.DataEngine.Core.Implementations;

public class CachedBaseRepository<TRepoEntity> : ICachedBaseRepository<TRepoEntity>
    where TRepoEntity : class, IRepoEntity, new()
{
    private readonly IBaseRepository<TRepoEntity> _base;
    private readonly ICacheAdapter _cacheAdapter;
    private readonly ICacheEventHandler<TRepoEntity> _eventHandler;
    private readonly ILogger<CachedBaseRepository<TRepoEntity>> _logger;
    private readonly ITracer _tracer;

    public CachedBaseRepository(IBaseRepository<TRepoEntity> @base, ILogger<CachedBaseRepository<TRepoEntity>> logger,
        ICacheAdapter cacheAdapter,
        ITracer tracer, ICacheEventHandler<TRepoEntity> eventHandler)
    {
        _logger = logger;
        _cacheAdapter = cacheAdapter;
        _tracer = tracer;
        _eventHandler = eventHandler;
        _base = @base;
        RepoEntityName = _base.RepoEntityName;
    }

    public string RepoEntityName { get; init; }

    public async Task<IEnumerable<TRepoEntity>> GetAsync(bool track = false, CancellationToken token = default)
    {
        return await Get(false, track, token);
    }

    public async Task<TRepoEntity> GetAsync(Guid id, bool track = false, CancellationToken token = default)
    {
        return await Get(id, false, track, token);
    }

    public async Task<IEnumerable<TRepoEntity>> GetAsync(Expression<Func<TRepoEntity, bool>> filter,
        bool track = false, CancellationToken token = default)
    {
        return await GetFiltered(filter, false, track, token);
    }

    public async Task<IEnumerable<TRepoEntity>> GetHashAsync(bool track = false, CancellationToken token = default)
    {
        return await Get(track, token: token);
    }

    public async Task<TRepoEntity> GetHashAsync(Guid id, bool track = false, CancellationToken token = default)
    {
        return await Get(id, track, token: token);
    }

    public async Task<IEnumerable<TRepoEntity>> GetHashAsync(Expression<Func<TRepoEntity, bool>> filter,
        bool track = false, CancellationToken token = default)
    {
        return await GetFiltered(filter, track, token: token);
    }

    public async Task<TRepoEntity> CreateAsync(TRepoEntity item, CancellationToken token = default)
    {
        var mainSpan = _tracer.BuildSpan("cached-repo.create").StartActive();
        try
        {
            var insertSpan = _tracer.BuildSpan("insertion").AsChildOf(mainSpan.Span)
                .Start();
            var created = await _base.CreateAsync(item, token);
            insertSpan.Finish();
            await _eventHandler.NotifyCreate(created, token);
            return created;
        }
        finally
        {
            mainSpan.Span.Finish();
        }
    }

    public async Task UpdateAsync(Guid id, TRepoEntity itemIn, CancellationToken token = default)
    {
        var mainSpan = _tracer.BuildSpan("cached-repo.update").StartActive();
        try
        {
            var replaceSpan = _tracer.BuildSpan("replace").AsChildOf(mainSpan.Span)
                .Start();
            await _base.UpdateAsync(id, itemIn, token);
            replaceSpan.Finish();
            await _eventHandler.NotifyUpdate(id, itemIn, token);
        }
        finally
        {
            mainSpan.Span.Finish();
        }
    }

    public async Task RemoveAsync(TRepoEntity itemIn, CancellationToken token = default)
    {
        var mainSpan = _tracer.BuildSpan("cached-repo.delete").StartActive();
        try
        {
            var deleteSpan = _tracer.BuildSpan("deletion").AsChildOf(mainSpan.Span)
                .Start();
            await _base.RemoveAsync(itemIn, token);
            deleteSpan.Finish();
            await _eventHandler.NotifyRemove(itemIn.Id, token);
        }
        finally
        {
            mainSpan.Span.Finish();
        }
    }

    public async Task RemoveAsync(Guid id, CancellationToken token = default)
    {
        var mainSpan = _tracer.BuildSpan("cached-repo.delete").StartActive();
        try
        {
            var deleteSpan = _tracer.BuildSpan("deletion").AsChildOf(mainSpan.Span)
                .Start();
            await _base.RemoveAsync(id, token);
            deleteSpan.Finish();
            await _eventHandler.NotifyRemove(id, token);
        }
        finally
        {
            mainSpan.Span.Finish();
        }
    }

    public async Task RemoveAsync(Expression<Func<TRepoEntity, bool>> filter, CancellationToken token = default)
    {
        var mainSpan = _tracer.BuildSpan("cached-repo.delete-filtered").StartActive();
        try
        {
            var deleteSpan = _tracer.BuildSpan("deletion-filtered")
                .AsChildOf(mainSpan.Span)
                .Start();
            await _base.RemoveAsync(filter, token);
            deleteSpan.Finish();
            await _eventHandler.NotifyRemove(token);
        }
        finally
        {
            mainSpan.Span.Finish();
        }
    }

    public async Task RemoveManyAsync(Guid[] ids, CancellationToken token = default)
    {
        var mainSpan = _tracer.BuildSpan("cached-repo.delete").StartActive();
        try
        {
            var deleteSpan = _tracer.BuildSpan("deletion").AsChildOf(mainSpan.Span)
                .Start();
            await _base.RemoveManyAsync(ids, token);
            deleteSpan.Finish();
            await _eventHandler.NotifyRemove(ids, token);
        }
        finally
        {
            mainSpan.Span.Finish();
        }
    }

    public async Task<bool> ExistsAsync(Expression<Func<TRepoEntity, bool>> filter, CancellationToken token = default)
    {
        // TODO: Add cache check first
        return await _base.ExistsAsync(filter, token);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken token = default)
    {
        // TODO: Add cache check first
        return await _base.ExistsAsync(id, token);
    }

    /// <summary>
    ///     Get items from cache
    /// </summary>
    /// <param name="hashMode">Get from HashSet</param>
    /// <param name="track">Should entity be tracked</param>
    /// <param name="token">Token for operation cancel</param>
    /// <returns>Entries of entity</returns>
    private async Task<IEnumerable<TRepoEntity>> Get(bool hashMode = true, bool track = false, CancellationToken token = default)
    {
        _logger.LogInformation("Requested all {RepoEntityName} items", RepoEntityName);
        var mainSpan = _tracer.BuildSpan("cached-repo.get-all").StartActive();
        try
        {
            token.ThrowIfCancellationRequested();
            var redisSpan = _tracer.BuildSpan("cached-repo.get.cache").Start();
            var redisResult = hashMode
                ? await _cacheAdapter.GetCollectionFromHashAsync<TRepoEntity>()
                : await _cacheAdapter.GetFromSingleKeyAsync<TRepoEntity>(RepoEntityName);
            redisSpan.Finish();
            if (redisResult != null)
            {
                var resultSpan = _tracer.BuildSpan("cached-repo.result.cache").AsChildOf(mainSpan.Span).Start();
                var conversionSpan = _tracer.BuildSpan("cached-repo.get-all-conversion.cache").AsChildOf(resultSpan).Start();
                var cacheEntities = redisResult.ToArray();
                conversionSpan.Finish();
                if (cacheEntities .Length > 0)
                {
                    _logger.LogInformation(
                        "Request of all {RepoEntityName} items returned {ItemCnt} items", RepoEntityName, cacheEntities.Length);
                    resultSpan.Finish();
                    return cacheEntities;
                }
                resultSpan.Finish();
            }

            _logger.LogWarning("Items of type {RepoEntityName} are not presented in cache", RepoEntityName);
            var dbSpan = _tracer.BuildSpan("cached-repo.get.db")
                .AsChildOf(mainSpan.Span).Start();
            var dbResult = await _base.GetAsync(track, token);
            dbSpan.Finish();
            await _eventHandler.NotifyMissing(token);

            var repoEntities = dbResult as TRepoEntity[] ?? dbResult.ToArray();
            _logger.LogInformation("Request of all {RepoEntityName} items returned {ItemCnt} items", RepoEntityName, repoEntities.Length);
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
        finally
        {
            mainSpan.Span.Finish();
        }
    }

    /// <summary>
    ///     Get items from cache
    /// </summary>
    /// <param name="hashMode">Get from HashSet</param>
    /// <param name="filter">Condition to filter</param>
    /// <param name="track">Should entity be tracked</param>
    /// <param name="token">Token for operation cancel</param>
    /// <returns>Entries of entity</returns>
    private async Task<IEnumerable<TRepoEntity>> GetFiltered(Expression<Func<TRepoEntity, bool>> filter,
        bool hashMode = true, bool track = false, CancellationToken token = default)
    {
        _logger.LogInformation("Requested filtered {RepoEntityName} items, filter: {@Filter}", RepoEntityName, filter);
        var mainSpan = _tracer.BuildSpan("cached-repo.get-filtered").StartActive();
        try
        {
            token.ThrowIfCancellationRequested();
            var redisResult = hashMode
                ? await _cacheAdapter.GetCollectionFromHashAsync<TRepoEntity>()
                : await _cacheAdapter.GetFromSingleKeyAsync<TRepoEntity>(RepoEntityName);

            if (redisResult != null)
            {
                var redisEntities = redisResult.Where(filter.Compile()).ToArray();
                if (redisEntities.Length > 0)
                {
                    _logger.LogInformation(
                        "Request of filtered {RepoEntityName} items returned {ItemCnt} items, filter: {@Filter}",
                        RepoEntityName, redisEntities.Length, filter);
                    return redisEntities;
                }
            }

            _logger.LogWarning("Items of type {RepoEntityName} are not presented in cache", RepoEntityName);
            var dbSpan = _tracer.BuildSpan("cached-repo.get-filtered.db")
                .AsChildOf(mainSpan.Span).Start();
            var dbResult = await _base.GetAsync(filter, track, token);
            dbSpan.Finish();
            await _eventHandler.NotifyMissing(token);

            var repoEntities = dbResult as TRepoEntity[] ?? dbResult.ToArray();
            _logger.LogInformation(
                "Request of filtered {RepoEntityName} items returned {repoEntities.Length} items, filter: {@Filter}",
                RepoEntityName, repoEntities.Length, filter);
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
                "There was an error during attempt to return filtered {RepoEntityName} items, filter: {@Filter}",
                RepoEntityName, filter);
            return default;
        }
        finally
        {
            mainSpan.Span.Finish();
        }
    }

    /// <summary>
    ///     Get item from cache
    /// </summary>
    /// <param name="id">Identifier of entry</param>
    /// <param name="hashMode">Get from HashSet</param>
    /// <param name="track">Should entity be tracked</param>
    /// <param name="token">Token for operation cancel</param>
    /// <returns>Entry of entity</returns>
    private async Task<TRepoEntity> Get(Guid id, bool hashMode = true, bool track = false, CancellationToken token = default)
    {
        var mainSpan = _tracer.BuildSpan("cached-repo.get").StartActive();
        try
        {
            var redisResult = hashMode
                ? await _cacheAdapter.GetSingleFromHashAsync<TRepoEntity>(id)
                : await _cacheAdapter.GetFromSingleKeyAsync<TRepoEntity>(RepoEntityName, id);

            if (redisResult != null)
            {
                _logger.LogInformation("Request of {RepoEntityName} with id: {Id} succeeded", RepoEntityName, id);
                return redisResult;
            }

            _logger.LogWarning("Item of type {RepoEntityName}  with id: {Id} is not presented in cache", RepoEntityName, id);
            var dbSpan = _tracer.BuildSpan("cached-repo.get.db")
                .AsChildOf(mainSpan.Span).Start();
            var dbResult = await _base.GetAsync(id, track, token);
            dbSpan.Finish();
            await _eventHandler.NotifyMissing(id, token);
            _logger.LogInformation("Request of {RepoEntityName} with id: {Id} succeeded", RepoEntityName, id);
            return dbResult;
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
        finally
        {
            mainSpan.Span.Finish();
        }
    }
}