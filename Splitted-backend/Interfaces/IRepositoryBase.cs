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

        Task CreateMultiple(IEnumerable<T> entities);

        Task<List<T>> GetAll();

        Task<List<T>> GetEntitiesByCondition(Expression<Func<T, bool>> expression);

        Task<T?> GetEntityOrDefaultByCondition(Expression<Func<T, bool>> expression, params (Expression<Func<T, object>> include, Expression<Func<object, object>>? thenInclude)[] includes);
    }
}
