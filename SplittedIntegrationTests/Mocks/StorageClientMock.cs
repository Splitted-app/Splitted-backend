using ExternalServices.StorageClient;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedIntegrationTests.Mocks
{
    public static class StorageClientMock
    {
        public static IStorageClient GetMockedStorageClient()
        {
            Mock<IStorageClient> storageClientMock = new Mock<IStorageClient>();

            storageClientMock.Setup(sc => sc.UploadProfilePicture(It.IsAny<string>()))
                .ReturnsAsync("https://splitted-storage.com/20b8a253-dc08-41d1-b34a-62bcbc9c12d5_profile_picture.jpg");  

            return storageClientMock.Object;
        }
    }
}
