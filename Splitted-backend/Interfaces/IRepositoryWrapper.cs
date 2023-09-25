namespace Splitted_backend.Interfaces
{
    public interface IRepositoryWrapper
    {
        ITransactionRepository Transactions { get; }

        Task SaveChanges();
    }
}
