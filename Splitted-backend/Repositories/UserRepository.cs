using Splitted_backend.DbContexts;
using Splitted_backend.Interfaces;
using Splitted_backend.Models.Entities;

namespace Splitted_backend.Repositories
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(SplittedDbContext splittedDbContext)
            : base(splittedDbContext)
        {
        }
    }
}
