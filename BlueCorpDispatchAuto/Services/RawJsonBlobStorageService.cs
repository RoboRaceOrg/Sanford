using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using BlueCorpDispatchAuto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

public class RawJsonBlobStorageService : IRawJsonBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<RawJsonBlobStorageService> _logger;
    private readonly string _rawJsonContainer;

    public RawJsonBlobStorageService(BlobServiceClient blobServiceClient, ILogger<RawJsonBlobStorageService> logger, IOptions<RawJsonBlobStorageServiceOptions> options)
    {
        _blobServiceClient = blobServiceClient;
        _logger = logger;
        _rawJsonContainer = options.Value.RawJsonContainer;
    }

    public async Task UploadJsonAsync(string fileName, string jsonData)
    {
        try
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_rawJsonContainer);
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.None);
            var blobClient = blobContainerClient.GetBlobClient(fileName);

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonData));
            await blobClient.UploadAsync(stream, overwrite: true);

            _logger.LogInformation($"JSON file {fileName} uploaded successfully to Azure Blob Storage.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error uploading JSON to Blob Storage: {ex.Message}");
        }
    }

    public virtual async Task<string?> DownloadJsonAsync(string fileName)
    {
        try
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_rawJsonContainer);
            var blobClient = blobContainerClient.GetBlobClient(fileName);

            if (await blobClient.ExistsAsync())
            {
                var response = await blobClient.DownloadContentAsync();
                return response.Value.Content.ToString();
            }

            _logger.LogWarning($"JSON file {fileName} not found in Azure Blob Storage.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error downloading JSON from Blob Storage: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> JsonExistsAsync(string fileName)
    {
        try
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_rawJsonContainer);
            var blobClient = blobContainerClient.GetBlobClient(fileName);
            return await blobClient.ExistsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking if JSON exists in Blob Storage: {ex.Message}");
            return false;
        }
    }
}