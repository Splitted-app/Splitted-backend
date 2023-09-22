using Splitted_backend.DbContexts;
using Splitted_backend.Interfaces;

namespace Splitted_backend.Repositories
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private SplittedDbContext splittedDbContext;

        public RepositoryWrapper(SplittedDbContext splittedDbContext)
        {
            this.splittedDbContext = splittedDbContext;
        }
        
        public async Task SaveChanges() => await splittedDbContext.SaveChangesAsync();
    }
}
