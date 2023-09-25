using Models.Entities;
using Splitted_backend.DbContexts;
using Splitted_backend.Interfaces;

namespace Splitted_backend.Repositories
{
    public class TransactionRepository : RepositoryBase<Transaction>, ITransactionRepository
    {
        public TransactionRepository(SplittedDbContext splittedDbContext) : base(splittedDbContext)
        { 
        }
    }
}
