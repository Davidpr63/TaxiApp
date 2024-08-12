using Azure.Data.Tables;
using Azure;
using ApiGatewayService.Models;

namespace ApiGatewayService.CloudStorageService
{
    public class DriversVerificationTableStorage
    {
        private readonly TableClient _tableClient;

        public DriversVerificationTableStorage(IConfiguration configuration)
        {
            var serviceClient = new TableServiceClient(configuration["AzureStorage:ConnectionString"]);
            _tableClient = serviceClient.GetTableClient(configuration["AzureStorage:DriversVerificationTable"]);
        }

        public async Task CreateVerificationAsync(DriverVerification driver)
        {
            await _tableClient.CreateIfNotExistsAsync();
            await _tableClient.AddEntityAsync(driver);
        }


        public async Task DeleteUserAsync(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task UpdateUserAsync(DriverVerification driver)
        {
            await _tableClient.UpdateEntityAsync(driver, ETag.All);
        }

        public async Task<List<DriverVerification>> RetrieveAllVerificationsAsync()
        {
            var drivers = new List<DriverVerification>();
            await foreach (var driver in _tableClient.QueryAsync<DriverVerification>())
            {
                drivers.Add(driver);
            }
            return drivers;
        }
    }
}
