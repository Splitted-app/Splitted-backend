using ExternalServices.EmailSender;
using FluentAssertions;
using Models.EmailModels;
using Moq;
using Moq.Protected;
using SplittedUnitTests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.ExternalServicesTests.EmailSenderTests
{
    public class EmailSenderTests
    {
        [Fact]
        public async Task Test_SendConfirmationEmail()
        {
            string token = "eUm2jrjRejRjtuyesKy9";
            string email = "mati@gmail.com";
            string emailVerificationUri = $"https://splitted-app.com/ConfirmEmail?token={token}&email=mati%40gmail.com";

            Mock<EmailSender> emailSenderMock = EmailSenderMock.GetMockedEmailSender();

            Func<Task> action = async () =>
            {
                await emailSenderMock.Object.SendConfirmationEmail(token, email);
            };

            await action.Should().NotThrowAsync();

            emailSenderMock.Protected()
                .Verify("SendEmail", Times.Once(), ItExpr.Is<EmailMessage>(em => 
                    em.To[0].Address.Equals(email) &&
                    em.To[0].Name.Equals("mati") && 
                    em.Subject.Equals("Confirmation email") && 
                    em.Values[0].placeHolder.Equals("[confirmation-link]") && 
                    em.Values[0].actualValue.Equals(emailVerificationUri)
                )
            );
        }
    }
}
