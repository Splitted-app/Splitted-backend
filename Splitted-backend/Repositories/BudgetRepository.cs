using Models.Entities;
using Splitted_backend.DbContexts;
using Splitted_backend.Interfaces;

namespace Splitted_backend.Repositories
{
    public class BudgetRepository : RepositoryBase<Budget>, IBudgetRepository
    {
        public BudgetRepository(SplittedDbContext splittedDbContext) : base(splittedDbContext)
        {  
        }
    }
}
