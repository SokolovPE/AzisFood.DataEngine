using System;
using AzisFood.DataEngine.Mongo.Extensions;
using AzisFood.DataEngine.Mongo.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AzisFood.DataEngine.Mongo;

/// <summary>
///     Helper class for automated connection registration
/// </summary>
public static class MongoConnectionConfigurator
{
    /// <summary>
    ///     Register all configured connections
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configuration"></param>
    /// <exception cref="Exception"></exception>
    internal static void RegisterConnections(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var config = configuration.GetSection(nameof(MongoConfiguration)).Get<MongoConfiguration>();
        if (config == null) throw new Exception("Mongo was not configured in application settings");

        foreach (var connect in config.Connections) serviceCollection.AddMongoConnect(connect);
    }
}