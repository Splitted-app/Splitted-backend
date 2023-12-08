using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Splitted_backend.Extensions
{
    public static class IQueryableExtension
    {
        public static IQueryable<T> IncludeMultiple<T>(this IQueryable<T> query, params (Expression<Func<T, object>> include,
            Expression<Func<object, object>>? thenInclude, Expression<Func<object, object>>? thenThenInclude)[] includes)
            where T : class
        {
            if (includes is not null)
                query = includes.Aggregate(query, (current, include) =>
                {
                    var includableCurrent = current.Include(include.include);
                    if (include.thenInclude is not null) includableCurrent = includableCurrent.ThenInclude(include.thenInclude);
                    if (include.thenThenInclude is not null) includableCurrent = includableCurrent.ThenInclude(include.thenThenInclude);
                    return includableCurrent;
                });

            return query;
        }
    }
}
