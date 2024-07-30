using Azure.Storage.Blobs;
using System.Text.RegularExpressions;

namespace AuthService.CloudStorageService
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _blobContainerClient;

        public BlobStorageService(IConfiguration configuration)
        {
            var blobServiceClient = new BlobServiceClient(configuration["AzureStorage:ConnectionString"]);
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(configuration["AzureStorage:BlobContainerName"]);
        }

        public async Task<string> UploadImageAsync(string base64Image)
        {
            await _blobContainerClient.CreateIfNotExistsAsync();

            // Extract Base64 part if it contains data URI scheme
            var base64Data = Regex.Match(base64Image, @"data:image/(?<type>.+?);base64,(?<data>.+)").Groups["data"].Value;
            if (string.IsNullOrEmpty(base64Data))
            {
                base64Data = base64Image; // Assume the input is just the base64 string without the data URI scheme
            }

            var bytes = Convert.FromBase64String(base64Data);
            var stream = new MemoryStream(bytes);
            var blobClient = _blobContainerClient.GetBlobClient(Guid.NewGuid().ToString());

            await blobClient.UploadAsync(stream);

            return blobClient.Uri.ToString();
        }
    }
}
