using Splitted_backend.DbContexts;
using Splitted_backend.Interfaces;

namespace Splitted_backend.Repositories
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private SplittedDbContext splittedDbContext;

        private ITransactionRepository transactions;

        private IBudgetRepository budgets;


        public RepositoryWrapper(SplittedDbContext splittedDbContext)
        {
            this.splittedDbContext = splittedDbContext;
        }


        public ITransactionRepository Transactions
        {
            get
            {
                transactions ??= new TransactionRepository(splittedDbContext);
                return transactions;
            }
        }

        public IBudgetRepository Budgets
        {
            get
            {
                budgets ??= new BudgetRepository(splittedDbContext);
                return budgets;
            }
        }

        public async Task SaveChanges() => await splittedDbContext.SaveChangesAsync();
    }
}
