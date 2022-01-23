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
    ///     Add postgres database context
    ///     Use if auto context registration is disabled
    /// </summary>
    /// <param name="serviceCollection">Collection of services</param>
    /// <param name="contextName">Name of context from configuration</param>
    public static IServiceCollection AddPostgresContext<TContext>(this IServiceCollection serviceCollection,
        string contextName)
        where TContext : DbContext
    {
        return serviceCollection
            .AddPooledDbContextFactory<TContext>((serviceProvider, options) =>
            {
                try
                {
                    var configs = serviceProvider.GetRequiredService<PgConfiguration>();
                    var config = configs.Connections.First(con =>
                        string.Equals(con.Alias, contextName, StringComparison.InvariantCultureIgnoreCase));
                    options.UseNpgsql(config.ConnectionString);
                }
                catch (InvalidOperationException e)
                {
                    throw new Exception(
                        $"Unable to configure {contextName} make sure that context is configured in application settings",
                        e);
                }
            })
            .AddTransient<DbContext, TContext>();
    }

    /// <summary>
    ///     Register postgres
    /// </summary>
    /// <param name="serviceCollection">Collection of services</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="engineConfiguration">Additional options</param>
    public static IServiceCollection AddPostgresSupport(this IServiceCollection serviceCollection,
        IConfiguration configuration, EngineConfiguration engineConfiguration = null)
    {
        if (engineConfiguration is {ContextContextAutoRegister: true})
            PgContextConfigurator.RegisterContexts(serviceCollection, configuration);

        return serviceCollection
            .Configure<PgConfiguration>(configuration.GetSection(nameof(PgConfiguration)))
            .AddSingleton(sp => sp.GetRequiredService<IOptions<PgConfiguration>>().Value)
            .AddSingleton<IDataAccess, PgDataAccess>()
            .AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>))
            .AddTransient(typeof(ICachedBaseRepository<>), typeof(CachedBaseRepository<>))
            .AddTransient(typeof(ICacheOperator<>), typeof(CacheOperator<>));
    }
}