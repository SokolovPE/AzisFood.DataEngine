using System;
using System.Linq;
using AzisFood.DataEngine.Abstractions.Interfaces;
using AzisFood.DataEngine.Core;
using AzisFood.DataEngine.Core.Implementations;
using AzisFood.DataEngine.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AzisFood.DataEngine.Postgres.Extensions;

public static class InitExtensions
{
    /// <summary>
    /// Register postgres options
    /// </summary>
    /// <param name="serviceCollection">Collection of services</param>
    /// <param name="configuration">Application configuration</param>
    public static void AddPostgresOptions(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<PgOptions>(configuration.GetSection(nameof(PgOptions)));
        serviceCollection.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<PgOptions>>().Value);
    }

    /// <summary>
    /// Add postgres database context
    /// </summary>
    /// <param name="serviceCollection">Collection of services</param>
    /// <param name="contextName">Name of context from configuration</param>
    public static void AddPostgresContext<TContext>(this IServiceCollection serviceCollection, string contextName)
        where TContext : DbContext
    {
        serviceCollection
            .AddPooledDbContextFactory<TContext>((serviceProvider, options) =>
            {
                try
                {
                    var configs = serviceProvider.GetRequiredService<PgOptions>();
                    var config = configs.Connections.First(con =>
                        string.Equals(con.ConnectionName, contextName, StringComparison.InvariantCultureIgnoreCase));
                    options.UseNpgsql(config.ConnectionString);
                }
                catch (InvalidOperationException e)
                {
                    throw new Exception(
                        $"Unable to configure {contextName} make sure that context is configured in application settings",
                        e);
                }
            });
        serviceCollection.AddTransient<DbContext, TContext>();
    }

    public static void AddPostgresSupport(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IDataAccess, PgDataAccess>();
        serviceCollection.AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        serviceCollection.AddTransient(typeof(ICachedBaseRepository<>), typeof(CachedBaseRepository<>));
        serviceCollection.AddTransient(typeof(ICacheOperator<>), typeof(CacheOperator<>));
    }
}