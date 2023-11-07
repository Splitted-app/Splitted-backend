using Microsoft.EntityFrameworkCore;
using Splitted_backend.DbContexts;
using Splitted_backend.Extensions;
using Splitted_backend.Interfaces;
using System.Linq.Expressions;

namespace Splitted_backend.Repositories
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        private SplittedDbContext splittedDbContext { get; set; }


        protected RepositoryBase(SplittedDbContext splittedDbContext)
        {
            this.splittedDbContext = splittedDbContext;
        }


        public void Create(T entity) => splittedDbContext.Set<T>().Add(entity);

        public void Delete(T entity) => splittedDbContext.Set<T>().Remove(entity);

        public void Update(T entity) => splittedDbContext.Set<T>().Update(entity);

        public void DeleteMultiple(IEnumerable<T> entities) => splittedDbContext.Set<T>().RemoveRange(entities);

        public async Task CreateMultiple(IEnumerable<T> entities) => await splittedDbContext.Set<T>().AddRangeAsync(entities);

        public async Task<List<T>> GetAll() => await splittedDbContext.Set<T>().ToListAsync();

        public async Task<List<T>> GetEntitiesByCondition(Expression<Func<T, bool>> expression) 
            => await splittedDbContext.Set<T>().Where(expression).ToListAsync();

        public async Task<T?> GetEntityOrDefaultByCondition(Expression<Func<T, bool>> expression, params (Expression<Func<T, object>> include, Expression<Func<object, object>>? thenInclude)[] includes)
            => await splittedDbContext.Set<T>().IncludeMultiple(includes).FirstOrDefaultAsync(expression);
    }
}
