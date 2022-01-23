using System;
using System.Collections.Generic;
using System.Linq;
using AzisFood.DataEngine.Core.Attributes;
using AzisFood.DataEngine.Postgres.Extensions;
using AzisFood.DataEngine.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AzisFood.DataEngine.Postgres;

/// <summary>
///     Helper class for automated context registration
/// </summary>
public static class PgContextConfigurator
{
    /// <summary>
    ///     Scan loaded assemblies for suitable contexts
    /// </summary>
    private static IEnumerable<Type> ScanContexts()
    {
        var foundContexts = new List<Type>();
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(assembly =>
            assembly.FullName != null && !assembly.FullName.Contains("Microsoft") &&
            !assembly.FullName.Contains("System"));
        foreach (var assembly in allAssemblies)
        {
            var contexts = assembly.GetTypes()
                .Where(type => type.IsClass && type.IsSubclassOf(typeof(DbContext)))
                .Where(type => Attribute.GetCustomAttribute(type, typeof(ConnectionSettings)) != null);

            foundContexts.AddRange(contexts);
        }

        return foundContexts;
    }

    /// <summary>
    ///     Register all context which can be found in assemblies
    /// </summary>
    internal static void RegisterContexts(IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        var scannedContexts = ScanContexts();
        var contexts = scannedContexts as Type[] ?? scannedContexts.ToArray();

        var options = configuration.GetSection(nameof(PgConfiguration)).Get<PgConfiguration>();
        if (options == null) throw new Exception("Postgres was not configured in application settings");

        var configuredConnectionNames =
            options.Connections
                .Select(connection => connection.Alias)
                .ToArray();

        // Map connection settings to contexts
        var contextConnectionSettingsMap = contexts.ToDictionary(context => context, context =>
            Attribute.GetCustomAttribute(context, typeof(ConnectionSettings)) as ConnectionSettings);

        // Find out which connections were configured but there's no dbContext for them
        var configurationsWithoutContexts = configuredConnectionNames.Where(connectionName =>
                !contextConnectionSettingsMap.Select(map => map.Value.Name).Contains(connectionName))
            .ToArray();

        if (configurationsWithoutContexts.Length > 0)
            Console.Error.WriteLine(
                $"Skipped configured connections {string.Join(", ", configurationsWithoutContexts)}" +
                " because there's no suitable dbContext for them," +
                " remove them from application configuration or create appropriate dbContexts for them");
        foreach (var context in contexts)
        {
            var connectionSettings = contextConnectionSettingsMap[context];
            if (connectionSettings == null || !configuredConnectionNames.Contains(connectionSettings.Name))
            {
                // Need to write log here...
                Console.Error.WriteLine(
                    $"Skipped discovered DbContext {context.FullName}," +
                    " but there's no configuration found, verify application configuration");
                continue;
            }

            typeof(InitExtensions)
                .GetMethod("AddPostgresContext")
                ?.MakeGenericMethod(context)
                .Invoke(context, new object[]
                {
                    serviceCollection,
                    connectionSettings.Name
                });
        }
    }
}