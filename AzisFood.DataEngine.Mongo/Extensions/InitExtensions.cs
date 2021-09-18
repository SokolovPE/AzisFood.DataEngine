using AzisFood.DataEngine.Interfaces;
using AzisFood.DataEngine.Mongo.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AzisFood.DataEngine.Mongo.Extensions
{
    public static class InitExtensions
    {
        public static void AddMongoDBSupport(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.Configure<MongoOptions>(configuration.GetSection(nameof(MongoOptions)));
            serviceCollection.AddSingleton<IMongoOptions>(sp =>
                sp.GetRequiredService<IOptions<MongoOptions>>().Value);
            serviceCollection.AddTransient(typeof(IBaseRepository<>), typeof(MongoBaseRepository<>));
            serviceCollection.AddTransient(typeof(ICachedBaseRepository<>), typeof(MongoCachedBaseRepository<>));
            serviceCollection.AddTransient(typeof(ICacheOperator<>), typeof(CacheOperator<>));
        }
    }
}