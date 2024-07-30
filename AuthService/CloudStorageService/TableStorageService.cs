using Azure.Data.Tables;
using Azure;
using AuthService.Models;

namespace AuthService.CloudStorageService
{
    public class TableStorageService
    {
        private readonly TableClient _tableClient;

        public TableStorageService(IConfiguration configuration)
        {
            var serviceClient = new TableServiceClient(configuration["AzureStorage:ConnectionString"]);
            _tableClient = serviceClient.GetTableClient(configuration["AzureStorage:TableName"]);
        }

        public async Task CreateUserAsync(User user)
        {
            await _tableClient.CreateIfNotExistsAsync();
            await _tableClient.AddEntityAsync(user);
        }

        public async Task DeleteUserAsync(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task UpdateUserAsync(User user)
        {
            await _tableClient.UpdateEntityAsync(user, ETag.All);
        }

        public async Task<List<User>> RetrieveAllUsersAsync()
        {
            var users = new List<User>();
            await foreach (var user in _tableClient.QueryAsync<User>())
            {
                users.Add(user);
            }
            return users;
        }
    }
}
