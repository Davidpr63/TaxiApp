using Azure.Data.Tables;
using Azure;
using UserService.Models;

namespace UserService.CloudStorageService
{
    public class RidesTableStorage
    {
        private readonly TableClient _tableClient;

        public RidesTableStorage(string connectionString, string tableName)
        {
            var serviceClient = new TableServiceClient(connectionString);
            _tableClient = serviceClient.GetTableClient(tableName);
        }

        public async Task CreateRideAsync(Ride driver)
        {
            await _tableClient.CreateIfNotExistsAsync();
            await _tableClient.AddEntityAsync(driver);
        }


        public async Task DeleteRIdeAsync(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task UpdateRideAsync(Ride driver)
        {
            await _tableClient.UpdateEntityAsync(driver, ETag.All);
        }

        public async Task<List<Ride>> RetrieveAllRidesAsync()
        {
            var rides = new List<Ride>();
            await foreach (var ride in _tableClient.QueryAsync<Ride>())
            {
                rides.Add(ride);
            }
            return rides;
        }
    }
}
