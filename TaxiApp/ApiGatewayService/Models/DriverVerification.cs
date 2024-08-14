using Azure;
using Azure.Data.Tables;

namespace ApiGatewayService.Models
{
    public class DriverVerification : ITableEntity
    {
        public string? UserId { get; set; }
        public string? DriversName { get; set; }
        public string? DriversLastname { get; set; }
        public string? DriversEmail { get; set; }
        public string? VerificationStatus { get; set; }
        public int? NumberOfRide { get; set; }
        public double? AverageRating { get; set; }

        public int? SumOfRating { get; set; }
        public bool? IsBlocked { get; set; }
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public DriverVerification()
        {
            PartitionKey = "DriversVerification";
            RowKey = "";
        }
        public double? CalculateAverage(int? sum, int? numberOfRide)
        {
            return sum / numberOfRide;
        }
    }
}
