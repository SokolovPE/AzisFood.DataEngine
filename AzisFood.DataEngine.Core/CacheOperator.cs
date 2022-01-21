using System;
using System.Linq;
using System.Threading.Tasks;
using AzisFood.CacheService.Redis.Interfaces;
using AzisFood.DataEngine.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AzisFood.DataEngine.Core
{
    public class CacheOperator<T> : ICacheOperator<T>
    {
        private readonly IRedisCacheService _cacheService;
        private readonly ILogger<CacheOperator<T>> _logger;
        private readonly IBaseRepository<T> _repository;
        private readonly string _repoEntityName;
        public CacheOperator(IRedisCacheService cacheService, IBaseRepository<T> repository,
            ILogger<CacheOperator<T>> logger)
        {
            _cacheService = cacheService;
            _repository = repository;
            _logger = logger;
            _repoEntityName = typeof(T).Name;
        }
        public async Task FullRecache(TimeSpan expiry, bool asHash = true)
        {
            var items = (await _repository.GetAsync()).ToList();
            
            if (asHash)
            {
                await _cacheService.HashDropAsync<T>();
                await _cacheService.HashSetAsync(items, CommandFlags.None);
            }
            else
            {
                await _cacheService.RemoveAsync(_repoEntityName);
                var cacheSetResult = await _cacheService.SetAsync(_repoEntityName, items, expiry, CommandFlags.None);
                if (!cacheSetResult)
                {
                    _logger.LogWarning($"Unable to refresh {_repoEntityName} cache");
                    throw new Exception($"Unable to refresh {_repoEntityName} cache");
                }
            }

            _logger.LogInformation($"Successfully refreshed {_repoEntityName} cache");
        }
    }
}