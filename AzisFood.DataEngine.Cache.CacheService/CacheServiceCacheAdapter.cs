using AzisFood.CacheService.Redis.Interfaces;
using AzisFood.DataEngine.Abstractions.Interfaces;
using StackExchange.Redis;

namespace AzisFood.DataEngine.Cache.CacheService;

/// <summary>
///     Adapter for AzisFood.CacheService
/// </summary>
public class CacheServiceCacheAdapter : ICacheAdapter
{
    private readonly IRedisCacheService _cacheService;
    public CacheServiceCacheAdapter(IRedisCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    /// <inheritdoc />
    public Task<IEnumerable<TEntity>?> GetCollectionFromHashAsync<TEntity>() where TEntity : class, IRepoEntity, new() =>
        _cacheService.HashGetAllAsync<TEntity>(CommandFlags.None);

    /// <inheritdoc />
    public Task<TEntity> GetSingleFromHashAsync<TEntity>(Guid key) where TEntity : class, IRepoEntity, new() =>
        _cacheService.HashGetAsync<TEntity>(key.ToString(), CommandFlags.None);

    /// <inheritdoc />
    public Task<IEnumerable<TEntity>> GetFromSingleKeyAsync<TEntity>(string entityName) where TEntity : class, IRepoEntity, new()
    {
        //Ienumerable or list?
        return _cacheService.GetAsync<IEnumerable<TEntity>>(entityName);
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetFromSingleKeyAsync<TEntity>(string entityName, Guid key) where TEntity : class, IRepoEntity, new()
    {
        return (await _cacheService.GetAsync<IEnumerable<TEntity>>(entityName))?.FirstOrDefault(
            x => x.Id == key);
    }

    /// <inheritdoc />
    public Task DropHashAsync<TEntity>() => _cacheService.HashDropAsync<TEntity>();

    /// <inheritdoc />
    public Task StoreItemsAsHashAsync<TEntity>(IEnumerable<TEntity> items) =>
        _cacheService.HashSetAsync(items, CommandFlags.None);

    /// <inheritdoc />
    public Task DropSingleKeyAsync<TEntity>(string entityName)
    {
        return _cacheService.RemoveAsync(entityName);
    }

    /// <inheritdoc />
    public Task<bool> StoreItemsAsSingleKeyAsync<TEntity>(string entityName, IEnumerable<TEntity> items, TimeSpan expiry)
    {
        return _cacheService.SetAsync(entityName, items, expiry, CommandFlags.None);
    }
}