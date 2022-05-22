using System;
using System.Linq;
using System.Linq.Expressions;

namespace AzisFood.DataEngine.Abstractions.Interfaces;

public interface IBaseQueryableRepository<TEntity> : IBaseRepository<TEntity>
{
    /// <summary>
    ///     Get items as queryable
    /// </summary>
    /// <returns>Queryable of item</returns>
    public IQueryable<TEntity> GetQueryable();
    
    /// <summary>
    ///     Get items as queryable by condition
    /// </summary>
    /// <param name="filter">Condition for item filtering</param>
    /// <returns>Queryable matching condition</returns>
    IQueryable<TEntity> GetQueryable(Expression<Func<TEntity, bool>> filter);
    
}