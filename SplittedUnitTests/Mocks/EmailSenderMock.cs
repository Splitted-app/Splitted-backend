using ExternalServices.EmailSender;
using Models.EmailModels;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.Mocks
{
    public static class EmailSenderMock
    {
        public static Mock<EmailSender> GetMockedEmailSender()
        {
            EmailConfiguration emailConfiguration = new EmailConfiguration
            {
                EmailVerificationUri = "https://splitted-app.com/ConfirmEmail"
            };
            
            Mock<EmailSender> emailSenderMock = new Mock<EmailSender>(emailConfiguration);

            emailSenderMock.Protected()
                .Setup<Task>("SendEmail", ItExpr.IsAny<EmailMessage>())
                .Returns(Task.CompletedTask)
                .Verifiable();

            return emailSenderMock;
        }
    }
}
