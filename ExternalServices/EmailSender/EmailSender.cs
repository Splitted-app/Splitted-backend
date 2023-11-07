﻿using MimeKit;
using MailKit.Net.Smtp;
using Models.EmailModels;
using ExternalServices.Extensions;

namespace ExternalServices.EmailSender
{
    public class EmailSender : IEmailSender
    {   
        private EmailConfiguration emailConfiguration { get; }

        
        public EmailSender(EmailConfiguration emailConfiguration)
        {
            this.emailConfiguration = emailConfiguration;
        }


        public async Task SendVerificationEmail(string token, string email)
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
            string subject = "Confirmation email link";
            string content = $"Click the link below to confirm your email: \n {emailVerificationUri}";

            await SendEmail(new EmailMessage(emailAddresses, subject, content));
        }

        private async Task SendEmail(EmailMessage emailMessage)
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