using Models.EmailModels;

namespace ExternalServices.EmailSender
{
    public interface IEmailSender
    {
        Task SendConfirmationEmail(string token, string email);
    }
}
