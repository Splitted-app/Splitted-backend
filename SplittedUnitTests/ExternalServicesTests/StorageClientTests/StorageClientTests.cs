using ExternalServices.Extensions;
using ExternalServices.StorageClient;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using SplittedUnitTests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.ExternalServicesTests.StorageClientTests
{
    public class StorageClientTests
    {
        [Fact]
        public async Task Test_UploadProfilePicture()
        {
            Mock<StorageClient> storageClientMock = StorageClientMock.GetMockedStorageClient();

            string encodedPicture = "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAMCAg";
            string? profilePictureUri = null;

            Func<Task> action = async () =>
            {
                profilePictureUri = await storageClientMock.Object
                .UploadProfilePicture(encodedPicture);
            };

            await action.Should().NotThrowAsync();

            storageClientMock.Protected()
                .Verify("IntializeBlobContainerClient", Times.Once(), ItExpr.IsAny<IConfiguration>());
            storageClientMock.Protected()
                .Verify("GetBytesFromEncodedPicture", Times.Once(), ItExpr.IsAny<string>());
            storageClientMock.Protected()
                .Verify("UploadToBlob", Times.Once(), ItExpr.IsAny<string>(), ItExpr.IsAny<byte[]>());

            profilePictureUri.Should().Be("https://picture_profile.com");
        }
    }
}
