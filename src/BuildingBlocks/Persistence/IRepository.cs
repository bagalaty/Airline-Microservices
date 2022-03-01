using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BuildingBlocks.Domain;

namespace BuildingBlocks.Persistence
{
    public interface IRepository<TEntity> : IDisposable where TEntity : class, IAggregate
    {
        Task<TEntity> GetAsync(Guid id);
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate);
        Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        IAsyncEnumerable<TEntity> GetAllAsync(Expression<Func<TEntity, bool>> predicate = null);
        Task<IEnumerable<TEntity>> BrowseAsync(Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null);
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate);
        Task DeleteAsync(Guid id);
        Task DeleteAsync(Expression<Func<TEntity, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
    }
}
