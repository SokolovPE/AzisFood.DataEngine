using System;
using System.Linq;
using AzisFood.DataEngine.Abstractions.Interfaces;
using AzisFood.DataEngine.Core;
using AzisFood.DataEngine.Core.Attributes;
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
    ///     Get registered DbContextFactory for given context
    /// </summary>
    public static IDbContextFactory<TContext> GetDbContextFactory<TContext>(this IServiceProvider serviceProvider)
        where TContext : DbContext
    {
        return serviceProvider.GetService<IDbContextFactory<TContext>>();
    }

    /// <summary>
    ///     Add postgres database context
    ///     Use if auto context registration is disabled
    /// </summary>
    /// <param name="serviceCollection">Collection of services</param>
    public static IServiceCollection AddPostgresContext<TContext>(this IServiceCollection serviceCollection)
        where TContext : DbContext
    {
        return serviceCollection
            .AddPooledDbContextFactory<TContext>((serviceProvider, options) =>
            {
                var contextType = typeof(TContext);
                try
                {
                    var configs = serviceProvider.GetRequiredService<PgConfiguration>();
                    var aliasAttribute =
                        Attribute.GetCustomAttribute(contextType, typeof(ConnectionAlias)) as ConnectionAlias;
                    if (aliasAttribute == null)
                        throw new ArgumentException(
                            $"Context {contextType.FullName} has no {nameof(ConnectionAlias)} attribute. Context is not supported");

                    var config = configs.Connections.First(con =>
                        string.Equals(con.Alias, aliasAttribute.Alias, StringComparison.InvariantCultureIgnoreCase));
                    options.UseNpgsql(config.ConnectionString);
                }
                catch (InvalidOperationException e)
                {
                    throw new Exception(
                        $"Unable to configure {contextType.FullName} make sure that" +
                        " context is configured in application settings",
                        e);
                }
            });
    }

    /// <summary>
    ///     Register postgres
    /// </summary>
    /// <param name="serviceCollection">Collection of services</param>
    /// <param name="configuration">Application configuration</param>
    public static IServiceCollection AddPostgresSupport(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        // Read config to check for auto registration
        var pgConfig = configuration.GetSection(nameof(PgConfiguration));
        var config = pgConfig.Get<PgConfiguration>();
        if (config is {AutoRegistration: true})
            PgContextConfigurator.RegisterContexts(serviceCollection, configuration);

        return serviceCollection
            .Configure<PgConfiguration>(pgConfig)
            .AddSingleton(sp => sp.GetRequiredService<IOptions<PgConfiguration>>().Value)
            .AddSingleton<IDataAccess, PgDataAccess>()
            .AddSingleton<IQueryableDataAccess, PgQueryableDataAccess>()
            .AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>))
            .AddTransient(typeof(ICachedBaseRepository<>), typeof(CachedBaseRepository<>))
            .AddTransient(typeof(ICacheOperator<>), typeof(CacheOperator<>));
    }
}