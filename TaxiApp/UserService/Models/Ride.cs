using Azure;
using Azure.Data.Tables;

namespace UserService.Models
{
    public class Ride : ITableEntity
    {
        public string? RowKey { get ; set; }
        public string? PickupAddress { get; set; }
        public string? DropOffAddress { get; set; }
        public string? RandomTime { get; set; }
        public int? RandomPrice { get; set; }
        public string? UserId { get; set; }
        public int? DriverId { get; set; }
        public bool? IsActive { get; set; }
        public string? PartitionKey { get ; set ; }
       
        public DateTimeOffset? Timestamp { get ; set ; }
        public ETag ETag { get ; set ; }

        public Ride()
        {
            PartitionKey = "RidesTable";
            RowKey = "";
        }
    }
}
