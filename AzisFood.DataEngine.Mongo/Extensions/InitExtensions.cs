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
    ///     Register mongo connection
    /// </summary>
    /// <param name="serviceCollection">Collection of services</param>
    /// <param name="connectConfigurationSettings">Mongo connect configuration</param>
    /// <returns></returns>
    public static IServiceCollection AddMongoConnect(this IServiceCollection serviceCollection,
        MongoConnectConfiguration connectConfigurationSettings) => serviceCollection.AddSingleton(provider =>
        new MongoClient(connectConfigurationSettings.ConnectionString).GetDatabase(
            connectConfigurationSettings.Database));

    /// <summary>
    ///     Register mongo connection
    /// </summary>
    /// <param name="serviceCollection">Collection of services</param>
    /// <param name="connectName">Name of connection from application settings</param>
    public static IServiceCollection AddMongoConnect(this IServiceCollection serviceCollection, string connectName)
    {
        return serviceCollection.AddSingleton(provider =>
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
    }

    /// <summary>
    ///     Register mongo
    /// </summary>
    /// <param name="serviceCollection">Collection of services</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="engineConfiguration">Additional options</param>
    public static IServiceCollection AddMongoSupport(this IServiceCollection serviceCollection,
        IConfiguration configuration, EngineConfiguration engineConfiguration = null)
    {
        // Register mapping of Guid to string of MongoDb
        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

        if (engineConfiguration is {ContextContextAutoRegister: true})
            MongoConnectionConfigurator.RegisterConnections(serviceCollection, configuration);
        
        return serviceCollection
            .Configure<MongoOptions>(configuration.GetSection(nameof(MongoOptions)))
            .AddSingleton(sp => sp.GetRequiredService<IOptions<MongoOptions>>().Value)
            .AddSingleton<IDataAccess, MongoDataAccess>()
            .AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>))
            .AddTransient(typeof(ICachedBaseRepository<>), typeof(CachedBaseRepository<>))
            .AddTransient(typeof(ICacheOperator<>), typeof(CacheOperator<>));
    }
}