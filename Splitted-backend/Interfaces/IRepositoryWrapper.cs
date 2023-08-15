namespace Splitted_backend.Interfaces
{
    public interface IRepositoryWrapper
    {
        IUserRepository User { get; }

        Task SaveChanges();
    }
}
