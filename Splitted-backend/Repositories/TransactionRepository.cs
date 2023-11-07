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


        public void FindDuplicates(List<Transaction> transactionsAdded, List<Transaction> budgetTransactions)
        {
            foreach (Transaction transactionAdded in transactionsAdded)
            {
                Transaction? duplicatedTransaction = budgetTransactions
                    .Where(bt => bt.Equals(transactionAdded) && bt.DuplicatedTransactionId is null && 
                        !transactionAdded.Id.Equals(bt.Id))
                    .FirstOrDefault();

                if (duplicatedTransaction is not null)
                    transactionAdded.DuplicatedTransactionId = duplicatedTransaction.Id;
            }
        }
    }
}
