using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Storage.Queues;
using Azure;
using Microsoft.Extensions.Configuration;

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

            // Initialize BlobServiceClient
            _blobServiceClient = new BlobServiceClient(storageConnectionString);

            // Initialize QueueClients
            _orderQueueClient = new QueueClient(storageConnectionString, configuration["AzureStorage:OrderQueue"]);
            _inventoryQueueClient = new QueueClient(storageConnectionString, configuration["AzureStorage:InventoryQueue"]);

            // Initialize TableServiceClient
            _tableServiceClient = new TableServiceClient(storageConnectionString);

            // Initialize ShareClient
            _shareClient = new ShareClient(storageConnectionString, configuration["AzureStorage:FileShare"]);
        }

        // Uploads a blob to the specified container
        public async Task UploadBlobAsync(string blobName, Stream content)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient("product-images");
                await containerClient.CreateIfNotExistsAsync(); // Ensure the container exists
                var blobClient = containerClient.GetBlobClient(blobName);
                await blobClient.UploadAsync(content, true);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                throw new ApplicationException($"Failed to upload blob: {blobName}", ex);
            }
        }

        // Inserts or updates an entity in the specified table
        public async Task InsertIntoTableAsync<T>(T entity, string tableName) where T : ITableEntity, new()
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            await tableClient.CreateIfNotExistsAsync();
            await tableClient.UpsertEntityAsync(entity);
        }

        // Sends a message to the specified queue
        public async Task EnqueueMessageAsync(string message, string queueName)
        {
            try
            {
                var queueClient = queueName == "order-queue" ? _orderQueueClient : _inventoryQueueClient;
                await queueClient.CreateIfNotExistsAsync(); // Ensure the queue exists
                await queueClient.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                throw new ApplicationException($"Failed to enqueue message to queue: {queueName}", ex);
            }
        }

        // Uploads a file to the specified file share
        public async Task UploadFileAsync(string fileName, Stream content)
        {
            try
            {
                // Replace with your actual directory name or path
                string directoryName = "product-image";
                var directoryClient = _shareClient.GetDirectoryClient(directoryName);

                // Ensure the directory exists
                await directoryClient.CreateIfNotExistsAsync();

                var fileClient = directoryClient.GetFileClient(fileName);

                // Create or overwrite the file
                await fileClient.CreateAsync(content.Length); // Create the file with the specified length
                await fileClient.UploadAsync(content); // Upload the file content (overwrite if exists)

                // Optional: Set file permissions or other attributes if needed
            }
            catch (RequestFailedException ex)
            {
                // Log the exception and handle the error
                Console.WriteLine($"RequestFailedException: {ex.Message}");
                throw new ApplicationException($"Failed to upload file: {fileName}", ex);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Console.WriteLine($"Exception: {ex.Message}");
                throw new ApplicationException($"Failed to upload file: {fileName}", ex);
            }
        }

        public TableClient GetTableClient(string CustomerProfileTable)
        {
            return _tableServiceClient.GetTableClient(CustomerProfileTable);
        }
    }
}