using System.ComponentModel.DataAnnotations;
using Azure;
using Azure.Data.Tables;

namespace CLDV7112.Models
{
    public class CustomerProfile : ITableEntity
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Other properties
        public string Name { get; set; }

        public string Email { get; set; }
    }
}