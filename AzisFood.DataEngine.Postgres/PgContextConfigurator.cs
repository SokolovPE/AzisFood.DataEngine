using System;
using System.Collections.Generic;
using System.Linq;
using AzisFood.DataEngine.Core.Attributes;
using AzisFood.DataEngine.Postgres.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AzisFood.DataEngine.Postgres;

/// <summary>
/// </summary>
public static class PgContextConfigurator
{
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
    internal static void RegisterContexts(IServiceCollection serviceCollection)
    {
        var contexts = ScanContexts();
        foreach (var context in contexts)
        {
            var connectionSettings =
                Attribute.GetCustomAttribute(context, typeof(ConnectionSettings)) as ConnectionSettings;
            if (connectionSettings == null) continue;

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