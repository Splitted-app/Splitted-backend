using Azure.Storage.Blobs;
using ExternalServices.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
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


        protected virtual void IntializeBlobContainerClient(IConfiguration configuration)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(configuration["ConnectionStrings:BlobStorage"]);
            blobContainerClient = blobServiceClient.GetBlobContainerClient("profile-pictures");
        }

        protected virtual byte[]? GetBytesFromEncodedPicture(string encodedPicture)
        {
            string encodedBytes = encodedPicture.Split(",")[1];

            Span<byte> buffer = new Span<byte>(new byte[encodedBytes.Length]);
            bool converted = Convert.TryFromBase64String(encodedBytes, buffer, out int bytesWritten);

            return converted ? buffer.ToArray() : null;
        }

        protected virtual async Task<string> UploadToBlob(string filename, byte[] bytes)
        {
            BlobClient blobClient = blobContainerClient.GetBlobClient(filename);
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                await blobClient.UploadAsync(stream, true);
            }

            return blobClient.Uri.ToString();
        }

        public async Task<string?> UploadProfilePicture(string encodedPicture)
        {
            byte[]? bytes = GetBytesFromEncodedPicture(encodedPicture);
            if (bytes is null)
                return null;

            string filename = Guid.NewGuid().ToString() + "_profile_picture" + 
                encodedPicture.GetExtensionFromBase64String();

            return await UploadToBlob(filename, bytes);
        }
    }
}
