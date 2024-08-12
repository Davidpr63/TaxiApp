using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;

using System.Text.RegularExpressions;

namespace UserService.CloudStorageService
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _blobContainerClient;

        public BlobStorageService(string connectionString, string blobContainerName)
        {
            var blobServiceClient = new BlobServiceClient(connectionString);
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            var blobClient = _blobContainerClient.GetBlobClient(Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream);
            }

            return blobClient.Uri.ToString();
        }
    }
}
