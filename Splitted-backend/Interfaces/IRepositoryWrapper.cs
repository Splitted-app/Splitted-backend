namespace Splitted_backend.Interfaces
{
    public interface IRepositoryWrapper
    {
        ITransactionRepository Transactions { get; }

        IBudgetRepository Budgets { get; }

        IGoalRepository Goals { get; }
        
        Task SaveChanges();
    }
}
