using Models.EmailModels;

namespace ExternalServices.EmailSender
{
    public interface IEmailSender
    {
        Task SendVerificationEmail(string token, string email);
    }
}
