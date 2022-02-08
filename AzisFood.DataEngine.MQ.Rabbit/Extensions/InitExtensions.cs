using AzisFood.DataEngine.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AzisFood.DataEngine.MQ.Rabbit.Extensions;

public static class InitExtensions
{
    /// <summary>
    ///     Register AzisFood.MQ.Rabbit cache event handler
    /// </summary>
    /// <param name="serviceCollection">Collection of services</param>
    public static IServiceCollection UseRabbitCacheEventHandler(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddTransient(typeof(ICacheEventHandler<>), typeof(RabbitCacheEventHandler<>));
    }
}