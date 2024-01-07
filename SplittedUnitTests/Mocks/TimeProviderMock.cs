using Moq;
using Splitted_backend.Utils.TimeProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.Mocks
{
    public static class TimeProviderMock
    {
        public static ITimeProvider GetMockedTimeProvider(DateTime today)
        {
            Mock<ITimeProvider> timeProviderMock = new Mock<ITimeProvider>();
            timeProviderMock.Setup(t => t.Today()).Returns(today);

            return timeProviderMock.Object;
        }
    }
}
