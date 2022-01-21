using System;
using System.Linq;
using AzisFood.DataEngine.Abstractions.Interfaces;
using AzisFood.DataEngine.Core;
using AzisFood.DataEngine.Core.Implementations;
using AzisFood.DataEngine.Mongo.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace AzisFood.DataEngine.Mongo.Extensions;

public static class InitExtensions
{
    /// <summary>
    ///     Register mongo options
    /// </summary>
    /// <param name="serviceCollection">Collection of services</param>
    /// <param name="configuration">Application configuration</param>
    public static IServiceCollection AddMongoOptions(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        return serviceCollection
            .Configure<MongoOptions>(configuration.GetSection(nameof(MongoOptions)))
            .AddSingleton(sp => sp.GetRequiredService<IOptions<MongoOptions>>().Value);
    }

    public static IServiceCollection AddMongoConnect(this IServiceCollection serviceCollection, string connectName)
        => serviceCollection.AddSingleton(provider =>
        {
            try
            {
                var configs = provider.GetRequiredService<MongoOptions>();
                var config = configs.Connections.First(con =>
                    string.Equals(con.ConnectionName, connectName, StringComparison.InvariantCultureIgnoreCase));
                return new MongoClient(config.ConnectionString).GetDatabase(config.Database);
            }
            catch (InvalidOperationException e)
            {
                throw new Exception(
                    $"Unable to configure {connectName} make sure that connect is configured in application settings",
                    e);
            }
        });
    
    public static IServiceCollection AddMongoSupport(this IServiceCollection serviceCollection)
    {
        // Register mapping of Guid to string of MongoDb
        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
        
        return serviceCollection.AddSingleton<IDataAccess, MongoDataAccess>()
            .AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>))
            .AddTransient(typeof(ICachedBaseRepository<>), typeof(CachedBaseRepository<>))
            .AddTransient(typeof(ICacheOperator<>), typeof(CacheOperator<>));
    }
}