using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ApiGatewayService.Models
{
    public enum TypeOfUser { Admin, User, Driver};
    public class User : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public TypeOfUser? TypeOfUser { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
     
        public string? ConfirmPassword { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? image { get; set; }
        public string? ImageUrl { get; set; }
        public string? VerifcationStatus { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public User()
        {
            PartitionKey = "UserTable";
            RowKey = "";
        }

        
    }
}
