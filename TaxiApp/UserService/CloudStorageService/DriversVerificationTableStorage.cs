using Azure.Data.Tables;
using Azure;
using UserService.Models;
using Microsoft.Extensions.Configuration;

namespace UserService.CloudStorageService
{
    public class DriversVerificationTableStorage
    {
        private readonly TableClient _tableClient;

        public DriversVerificationTableStorage(string connectionString, string tableName)
        {
            var serviceClient = new TableServiceClient(connectionString);
            _tableClient = serviceClient.GetTableClient(tableName);
        }

        public async Task CreateVerificationAsync(DriverVerification driver)
        {
            await _tableClient.CreateIfNotExistsAsync();
            await _tableClient.AddEntityAsync(driver);
        }


        public async Task DeleteVerificationAsync(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task UpdateVerificationAsync(DriverVerification driver)
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
