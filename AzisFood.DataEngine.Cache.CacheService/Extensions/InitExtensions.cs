using AzisFood.DataEngine.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AzisFood.DataEngine.Cache.CacheService.Extensions;

public static class InitExtensions
{
    /// <summary>
    ///     Register CacheService adapter
    /// </summary>
    /// <param name="serviceCollection">Collection of services</param>
    public static IServiceCollection UseCacheServiceAdapter(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<ICacheAdapter, CacheServiceCacheAdapter>();
    }
}