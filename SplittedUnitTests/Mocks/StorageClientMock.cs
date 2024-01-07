using ExternalServices.StorageClient;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.Mocks
{
    public static class StorageClientMock
    {
        public static Mock<StorageClient> GetMockedStorageClient()
        {
            Mock<StorageClient> storageClientMock = new Mock<StorageClient>(It.IsAny<IConfiguration>());

            storageClientMock.Protected()
                .Setup("IntializeBlobContainerClient", ItExpr.IsAny<IConfiguration>())
                .Verifiable();

            storageClientMock.Protected()
                .Setup<byte[]?>("GetBytesFromEncodedPicture", ItExpr.IsAny<string>())
                .Returns(new byte[5])
                .Verifiable();

            storageClientMock.Protected()
                .Setup<Task<string>>("UploadToBlob", ItExpr.IsAny<string>(), ItExpr.IsAny<byte[]>())
                .ReturnsAsync("https://picture_profile.com")
                .Verifiable();

            return storageClientMock;
        }
    }
}
