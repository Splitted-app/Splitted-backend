using Microsoft.EntityFrameworkCore.ChangeTracking;
using MimeKit;
using MailKit.Net.Smtp;
using Models.EmailModels;
using Splitted_backend.Interfaces;

namespace Splitted_backend.ExternalServices
{
    public class EmailSender : IEmailSender
    {   
        private EmailConfiguration emailConfiguration { get; }

        
        public EmailSender(EmailConfiguration emailConfiguration)
        {
            this.emailConfiguration = emailConfiguration;
        }


        public async Task SendEmail(EmailMessage emailMessage)
        {
            MimeMessage message = CreateEmailMessage(emailMessage);
            await SendAsync(message);
        }

        private MimeMessage CreateEmailMessage(EmailMessage emailMessage)
        {
            MimeMessage message = new MimeMessage();

            message.From.Add(new MailboxAddress(emailConfiguration.DisplayName, emailConfiguration.From));
            message.To.AddRange(emailMessage.To);
            message.Subject = emailMessage.Subject;
            message.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = emailMessage.Content };

            return message;
        }

        private async Task SendAsync(MimeMessage message)
        {
            using (SmtpClient smtpClient = new SmtpClient())
            {
                try
                {
                    await smtpClient.ConnectAsync(emailConfiguration.SmtpServer, emailConfiguration.Port, true);
                    smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
                    await smtpClient.AuthenticateAsync(emailConfiguration.UserName, emailConfiguration.Password);
                    await smtpClient.SendAsync(message);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    await smtpClient.DisconnectAsync(true);
                    smtpClient.Dispose();
                }
            }
        }
    }
}
