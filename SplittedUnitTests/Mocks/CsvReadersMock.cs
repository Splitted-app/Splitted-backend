using CsvConversion.Readers;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.Mocks
{
    public static class CsvReadersMock
    {
        public static Mock<T> GetMockedCsvReader<T>(string filePath) where T : BaseCsvReader
        {
            Mock<T> csvReaderMock = new Mock<T>(It.IsAny<IFormFile>());
            csvReaderMock.CallBase = true;

            csvReaderMock.Protected()
                .Setup<string>("SaveCsvFile")
                .Returns(filePath)
                .Verifiable();

            csvReaderMock.Protected()
                .Setup("DeleteCsvFile", ItExpr.IsAny<string>())
                .Verifiable();

            return csvReaderMock;
        }
    }
}
