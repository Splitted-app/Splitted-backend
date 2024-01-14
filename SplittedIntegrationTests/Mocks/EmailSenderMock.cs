using ExternalServices.EmailSender;
using ExternalServices.StorageClient;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedIntegrationTests.Mocks
{
    public static class EmailSenderMock
    {
        public static IEmailSender GetMockedEmailSender()
        {
            Mock<IEmailSender> emailSenderMock = new Mock<IEmailSender>();

            emailSenderMock.Setup(es => es.SendConfirmationEmail(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            return emailSenderMock.Object;
        }
    }
}
