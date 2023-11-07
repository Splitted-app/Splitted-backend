using Models.Entities;

namespace Splitted_backend.Interfaces
{
    public interface ITransactionRepository : IRepositoryBase<Transaction>
    {
        void FindDuplicates(List<Transaction> transactionsAdded, List<Transaction> budgetTransactions);
    }
}
