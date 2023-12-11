using Models.Entities;
using Splitted_backend.DbContexts;
using Splitted_backend.Interfaces;

namespace Splitted_backend.Repositories
{
    public class GoalRepository : RepositoryBase<Goal>, IGoalRepository
    {
        public GoalRepository(SplittedDbContext splittedDbContext) : base(splittedDbContext)
        {
        }
    }
}
