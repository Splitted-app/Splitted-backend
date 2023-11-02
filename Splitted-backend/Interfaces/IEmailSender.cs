using Models.EmailModels;

namespace Splitted_backend.Interfaces
{
    public interface IEmailSender
    {
        Task SendVerificationEmail(string token, string email);
    }
}
