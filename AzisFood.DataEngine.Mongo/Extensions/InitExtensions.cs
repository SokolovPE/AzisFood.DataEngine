using System;
using System.Linq;
using AzisFood.DataEngine.Abstractions.Interfaces;
using AzisFood.DataEngine.Core;
using AzisFood.DataEngine.Core.Implementations;
using AzisFood.DataEngine.Mongo.Models;
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
    ///     Register mongo connection
    /// </summary>
    /// <param name="serviceCollection">Collection of services</param>
    /// <param name="connectConfigurationSettings">Mongo connect configuration</param>
    /// <returns></returns>
    public static IServiceCollection AddMongoConnect(this IServiceCollection serviceCollection,
        MongoConnectConfiguration connectConfigurationSettings)
    {
        return serviceCollection.AddSingleton(provider =>
            new MongoClient(connectConfigurationSettings.ConnectionString).GetDatabase(
                connectConfigurationSettings.GetMongoUrl.DatabaseName));
    }

    /// <summary>
    ///     Register mongo connection
    /// </summary>
    /// <param name="serviceCollection">Collection of services</param>
    /// <param name="connectionAlias">Name of connection from application settings</param>
    public static IServiceCollection AddMongoConnect(this IServiceCollection serviceCollection, string connectionAlias)
    {
        return serviceCollection.AddSingleton(provider =>
        {
            try
            {
                var configs = provider.GetRequiredService<MongoConfiguration>();
                var config = configs.Connections.First(con =>
                    string.Equals(con.Alias, connectionAlias, StringComparison.InvariantCultureIgnoreCase));
                
                return new MongoClient(config.ConnectionString).GetDatabase(config.GetMongoUrl.DatabaseName);
            }
            catch (InvalidOperationException e)
            {
                throw new Exception(
                    $"Unable to configure {connectionAlias} make sure that connect is configured in application settings",
                    e);
            }
        });
    }

    /// <summary>
    ///     Register mongo
    /// </summary>
    /// <param name="serviceCollection">Collection of services</param>
    /// <param name="configuration">Application configuration</param>
    public static IServiceCollection AddMongoSupport(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        // Register mapping of Guid to string of MongoDb
        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

        // Read config to check for auto registration
        var pgConfig = configuration.GetSection(nameof(MongoConfiguration));
        var config = pgConfig.Get<MongoConfiguration>();
        if (config is {AutoRegistration: true})
            MongoConnectionConfigurator.RegisterConnections(serviceCollection, configuration);

        return serviceCollection
            .Configure<MongoConfiguration>(pgConfig)
            .AddSingleton(sp => sp.GetRequiredService<IOptions<MongoConfiguration>>().Value)
            .AddSingleton<IDataAccess, MongoDataAccess>()
            .AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>))
            .AddTransient(typeof(ICachedBaseRepository<>), typeof(CachedBaseRepository<>))
            .AddTransient(typeof(ICacheOperator<>), typeof(CacheOperator<>));
    }
}