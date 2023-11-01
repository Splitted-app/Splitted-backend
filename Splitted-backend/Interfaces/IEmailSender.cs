using Models.EmailModels;

namespace Splitted_backend.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmail(EmailMessage emailMessage);
    }
}
