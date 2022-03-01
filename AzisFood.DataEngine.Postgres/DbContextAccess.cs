using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace AzisFood.DataEngine.Postgres;

/// <summary>
///     Helper to work with DbContextPools
/// </summary>
public static class DbContextAccess
{
    private static readonly Dictionary<Type, Func<Type, IServiceProvider, DbContext>> ContextFactories;
    static DbContextAccess()
    {
        var contexts = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())
            .Where(type=>type.BaseType == typeof(DbContext))
            .ToHashSet();

        var factoryMethods = contexts.ToDictionary(x => x, GenerateDbContext);

        ContextFactories = contexts.SelectMany(x => x.GetProperties().Where(p =>
                p.CanRead && p.CanWrite && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)))
            .ToDictionary(x => x.PropertyType.GetGenericArguments().Single(),
                x => factoryMethods[x.DeclaringType ?? throw new Exception()]);
    }

    private static DbContext Create(Type entity, IServiceProvider provider) => ContextFactories
        .TryGetValue(entity, out var factory)
        ? factory(entity, provider)
        : null;

    public static DbContext Create<TEntity>(IServiceProvider provider) => Create(typeof(TEntity), provider);
    
    /// <summary>
    ///     Generate database context
    /// </summary>
    private static Func<Type, IServiceProvider, DbContext> GenerateDbContext(Type contextType)
    {
        var dbContextType = typeof(DbContext);
        var providerParameter = Expression.Parameter(typeof(IServiceProvider), "provider");
        var entityTypeParameter = Expression.Parameter(typeof(Type), "entityType");
        
        var getServiceMethod = typeof(IServiceProvider).GetMethod(nameof(IServiceProvider.GetService),
                                   BindingFlags.Public | BindingFlags.Instance) ??
                               throw new Exception("Unable to locate GetService method in IServiceProvider");
        
        var contextFactoryType = typeof(IDbContextFactory<>).MakeGenericType(contextType);
        var factory = Expression.Variable(contextFactoryType, "dbContextFactory");

        var createMethod = contextFactoryType.GetMethod(nameof(IDbContextFactory<DbContext>.CreateDbContext)) ??
                           throw new Exception(
                               $"Error while trying to generate DbContextFactory for {contextFactoryType.Name}({contextType.Name})");
        
        var assignFactoryExpression = Expression.Assign(factory,
            Expression.Convert(
                Expression.Call(providerParameter, getServiceMethod, Expression.Constant(contextFactoryType)),
                contextFactoryType));

        var labelTarget = Expression.Label(dbContextType);
        var returnExpression = Expression.Return(labelTarget,
            Expression.Convert(Expression.Call(factory, createMethod), dbContextType), dbContextType);
        var label = Expression.Label(labelTarget, Expression.Constant(null, dbContextType));
        var body = new List<Expression> {assignFactoryExpression, returnExpression, label};
        var lambda =
            Expression.Lambda<Func<Type, IServiceProvider, DbContext>>(Expression.Block(new[] {factory}, body),
                entityTypeParameter, providerParameter);
        return lambda.Compile();
    }
}