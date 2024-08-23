using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Storage.Queues;
using Azure;

namespace CLDV7112.Services
{
    public class AzureStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly QueueClient _orderQueueClient;
        private readonly QueueClient _inventoryQueueClient;
        private readonly TableServiceClient _tableServiceClient;
        private readonly ShareClient _shareClient;

        public AzureStorageService(IConfiguration configuration)
        {
            string storageConnectionString =
                $"DefaultEndpointsProtocol=https;AccountName=abcretailcustomers;AccountKey=/FH3Mr+EVXsBqCsuLQ7VkYXxrTSLt5iSdGQQ0zoPMMSKqv8Ndi4+0aoX2HlDIQ85eq53cmyHLcWA+AStE97a+Q==;EndpointSuffix=core.windows.net";

            _blobServiceClient = new BlobServiceClient(storageConnectionString);
            _orderQueueClient = new QueueClient(storageConnectionString, configuration["AzureStorage:OrderQueue"]);
            _inventoryQueueClient =
                new QueueClient(storageConnectionString, configuration["AzureStorage:InventoryQueue"]);
            _tableServiceClient = new TableServiceClient(storageConnectionString);
            _shareClient = new ShareClient(storageConnectionString, configuration["AzureStorage:FileShare"]);
        }

        public async Task UploadBlobAsync(string blobName, Stream content)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("product-images");
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(content, true);
        }

        public async Task InsertIntoTableAsync<T>(T entity, string tableName) where T : ITableEntity, new()
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            await tableClient.UpsertEntityAsync(entity);
        }

        public async Task EnqueueMessageAsync(string message, string queueName)
        {
            var queueClient = queueName == "order-queue" ? _orderQueueClient : _inventoryQueueClient;
            await queueClient.SendMessageAsync(message);
        }

        public async Task UploadFileAsync(string fileName, Stream content)
        {
            var directoryClient = _shareClient.GetDirectoryClient("");
            var fileClient = directoryClient.GetFileClient(fileName);
            await fileClient.CreateAsync(content.Length);
            await fileClient.UploadRangeAsync(new HttpRange(0, content.Length), content);
        }
    }
}