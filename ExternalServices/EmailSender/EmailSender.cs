using MimeKit;
using MailKit.Net.Smtp;
using Models.EmailModels;
using ExternalServices.Extensions;
using MimeKit.Utils;

namespace ExternalServices.EmailSender
{
    public class EmailSender : IEmailSender
    {   
        private EmailConfiguration emailConfiguration { get; }

        private string templatesCatalogPath { get; }


        public EmailSender(EmailConfiguration emailConfiguration)
        {
            this.emailConfiguration = emailConfiguration;
            this.templatesCatalogPath = Path.Combine(Directory.GetCurrentDirectory(),
                "../ExternalServices/EmailSender/Templates");
        }


        public async Task SendConfirmationEmail(string token, string email)
        {
            Uri emailVerificationUri = new Uri(emailConfiguration.EmailVerificationUri);
            emailVerificationUri = emailVerificationUri.AddParameter("token", token);
            emailVerificationUri = emailVerificationUri.AddParameter("email", email);

            List<EmailAddress> emailAddresses = new List<EmailAddress>()
            {
                new EmailAddress
                {
                    DisplayName = email.Split("@")[0],
                    Address = email
                }
            };
            string subject = "Confirmation email";
            string htmlTemplate = Path.Combine(templatesCatalogPath, "ConfirmationEmailTemplate.html");
            List<(string placeHolder, string actualValue)> values = new List<(string placeHolder, string actualValue)> 
                { ("[confirmation-link]", emailVerificationUri.ToString()) };

            await SendEmail(new EmailMessage(emailAddresses, subject, htmlTemplate, values));
        }

        private async Task SendEmail(EmailMessage emailMessage)
        {
            MimeMessage message = CreateMimeMessage(emailMessage);
            await SendAsync(message);
        }

        private MimeMessage CreateMimeMessage(EmailMessage emailMessage)
        {
            MimeMessage message = new MimeMessage();

            message.From.Add(new MailboxAddress(emailConfiguration.DisplayName, emailConfiguration.From));
            message.To.AddRange(emailMessage.To);
            message.Subject = emailMessage.Subject;
            message.Body = ReadHtmlBody(emailMessage.HtmlPath, emailMessage.Values);

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

        private MimeEntity ReadHtmlBody(string htmlPath, List<(string placeHolder, string actualValue)> values)
        {
            BodyBuilder bodyBuilder = new BodyBuilder();

            using (StreamReader streamReader = File.OpenText(htmlPath))
            {
                bodyBuilder.HtmlBody = streamReader.ReadToEnd();
            }

            values.ForEach(v =>
            {
                bodyBuilder.HtmlBody = bodyBuilder.HtmlBody.Replace(v.placeHolder, v.actualValue);
            });
            return bodyBuilder.ToMessageBody();
        }
    }
}
