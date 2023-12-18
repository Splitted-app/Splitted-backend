using Azure.Storage.Blobs;
using ExternalServices.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalServices.StorageClient
{
    public class StorageClient : IStorageClient
    {
        private BlobContainerClient blobContainerClient { get; set; }


        public StorageClient(IConfiguration configuration)
        {
            IntializeBlobContainerClient(configuration);
        }


        private void IntializeBlobContainerClient(IConfiguration configuration)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(configuration["ConnectionStrings:BlobStorage"]);
            blobContainerClient = blobServiceClient.GetBlobContainerClient("profile-pictures");
        }

        private byte[]? GetBytesFromEncodedPicture(string encodedPicture)
        {
            string encodedBytes = encodedPicture.Split(",")[1];

            Span<byte> buffer = new Span<byte>(new byte[encodedBytes.Length]);
            bool converted = Convert.TryFromBase64String(encodedBytes, buffer, out int bytesWritten);

            return converted ? buffer.ToArray() : null;
        }

        public async Task<string?> UploadProfilePicture(string encodedPicture)
        {
            byte[]? bytes = GetBytesFromEncodedPicture(encodedPicture);
            if (bytes is null)
                return null;

            string filename = Guid.NewGuid().ToString() + "_profile_picture" + 
                encodedPicture.GetExtensionFromBase64String();

            BlobClient blobClient = blobContainerClient.GetBlobClient(filename);
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                await blobClient.UploadAsync(stream, true);
            }

            return blobClient.Uri.ToString();
        }


    }
}
