using Splitted_backend.Interfaces;

namespace Splitted_backend.Repositories
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private SplittedDbContext splittedDbContext;
        private IUserRepository? user;

        public RepositoryWrapper(SplittedDbContext splittedDbContext)
        {
            this.splittedDbContext = splittedDbContext;
        }

        public IUserRepository User
        {
            get
            {
                if (user is null) 
                    user = new UserRepository(splittedDbContext);
                return user;
            }
        }
      
        public async Task SaveChanges() => await splittedDbContext.SaveChangesAsync();
    }
}
