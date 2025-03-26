using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace BlueCorpDispatchAuto
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<BlobStorageService> _logger;
        private readonly string _processedCsvContainer;

        public BlobStorageService(BlobServiceClient blobServiceClient, ILogger<BlobStorageService> logger, IOptions<BlobStorageServiceOptions> options)
        {
            _blobServiceClient = blobServiceClient;
            _logger = logger;
            _processedCsvContainer = options.Value.ProcessedCsvContainer;
        }

        public async Task UploadCsvToBlobAsync(string fileName, string csvData)
        {
            try
            {
                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_processedCsvContainer);
                await blobContainerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.None);
                var blobClient = blobContainerClient.GetBlobClient(fileName);

                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvData));
                await blobClient.UploadAsync(stream, overwrite: true);

                _logger.LogInformation($"CSV {fileName} uploaded successfully to Azure Blob Storage.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading CSV to Blob Storage: {ex.Message}");
            }
        }
    }
}

