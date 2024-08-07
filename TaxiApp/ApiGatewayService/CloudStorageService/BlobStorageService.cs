using Azure.Storage.Blobs;
using System.Text.RegularExpressions;

namespace ApiGatewayService.CloudStorageService
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _blobContainerClient;

        public BlobStorageService(IConfiguration configuration)
        {
            var blobServiceClient = new BlobServiceClient(configuration["AzureStorage:ConnectionString"]);
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(configuration["AzureStorage:BlobContainerName"]);
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
