using Splitted_backend.DbContexts;
using System.Linq.Expressions;

namespace Splitted_backend.Interfaces
{
    public interface IRepositoryBase<T>
    {
        void Create(T entity);

        void Update(T entity);

        void Delete(T entity);

        void DeleteMultiple(IEnumerable<T> entities);

        Task CreateMultipleAsync(IEnumerable<T> entities);

        Task<List<T>> GetAllAsync();

        Task<List<T>> GetEntitiesByConditionAsync(Expression<Func<T, bool>> expression);

        T? GetEntityOrDefaultByCondition(Expression<Func<T, bool>> expression, params (Expression<Func<T, object>> include,
            Expression<Func<object, object>>? thenInclude, Expression<Func<object, object>>? thenThenInclude)[] includes);

        Task<T?> GetEntityOrDefaultByConditionAsync(Expression<Func<T, bool>> expression, params (Expression<Func<T, object>> include, 
            Expression<Func<object, object>>? thenInclude, Expression<Func<object, object>>? thenThenInclude)[] includes);
    }
}
