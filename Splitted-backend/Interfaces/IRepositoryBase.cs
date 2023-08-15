using System.Linq.Expressions;

namespace Splitted_backend.Interfaces
{
    public interface IRepositoryBase<T>
    {
        Task<List<T>> GetAll();

        Task<List<T>> GetEntitiesByCondition(Expression<Func<T, bool>> expression);

        Task<T?> GetEntityOrDefaultByCondition(Expression<Func<T, bool>> expression);

        void Create(T entity);

        void Update(T entity);

        void Delete(T entity);
    }
}
