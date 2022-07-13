using System;
using System.Linq;
using System.Threading.Tasks;
using AzisFood.DataEngine.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;

namespace AzisFood.DataEngine.Core;

public class CacheOperator<T> : ICacheOperator<T>
{
    private readonly ICacheAdapter _cacheService;
    private readonly ILogger<CacheOperator<T>> _logger;
    private readonly string _repoEntityName;
    private readonly IBaseRepository<T> _repository;

    public CacheOperator(ICacheAdapter cacheService, IBaseRepository<T> repository,
        ILogger<CacheOperator<T>> logger)
    {
        _cacheService = cacheService;
        _repository = repository;
        _logger = logger;
        _repoEntityName = typeof(T).Name;
    }

    public async Task FullRecache(TimeSpan expiry, bool asHash = true)
    {
        try
        {
            var items = (await _repository.GetAsync(false)).ToList();

            if (asHash)
            {
                await _cacheService.DropHashAsync<T>();
                await _cacheService.StoreItemsAsHashAsync(items);
            }
            else
            {
                await _cacheService.DropSingleKeyAsync<T>(_repoEntityName);
                var cacheSetResult = await _cacheService.StoreItemsAsSingleKeyAsync(_repoEntityName, items, expiry);
                if (!cacheSetResult)
                {
                    _logger.LogWarning("Unable to refresh {@RepoEntityName} cache", _repoEntityName);
                    throw new Exception($"Unable to refresh {_repoEntityName} cache");
                }
            }

            _logger.LogInformation("Successfully refreshed {@RepoEntityName} cache", _repoEntityName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during recache {@RepoEntityName} entity", _repoEntityName);
        }
    }
}