using Azure.Data.Tables;
using Azure;
using AuthenticationService.Models;
using Microsoft.Extensions.Configuration;

namespace AuthenticationService.CloudStorageService
{
    public class TableStorageService
    {
        private readonly TableClient _tableClient;

        public TableStorageService(string connectionString, string tableName)
        {
            
            var serviceClient = new TableServiceClient(connectionString);
            _tableClient = serviceClient.GetTableClient(tableName);
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
