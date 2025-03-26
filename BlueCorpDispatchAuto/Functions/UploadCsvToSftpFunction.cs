using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BlueCorpDispatchAuto
{
    public class UploadCsvToSftpFunction
    {
        private readonly ILogger<UploadCsvToSftpFunction> _logger;
        private readonly ISftpService _sftpService;
        private readonly BlobServiceClient _blobServiceClient; // Inject BlobServiceClient
        private readonly string _archiveCsvContainer;
        private readonly string _processedFolder = "bluecorp-processed";
        private readonly string _failedFolder = "bluecorp-failed";

        public UploadCsvToSftpFunction(ILogger<UploadCsvToSftpFunction> logger, ISftpService sftpService, BlobServiceClient blobServiceClient)
        {
            _logger = logger;
            _sftpService = sftpService;
            _blobServiceClient = blobServiceClient; // Assign injected BlobServiceClient
            _archiveCsvContainer = "dispatch-archive";
        }

        [Function("UploadCsvToSftp")]
        public async Task RunAsync(
            [BlobTrigger("%ProcessedCsvContainer%/{name}", Connection = "AzureWebJobsStorage")] Stream blobStream,
            string name)
        {
            _logger.LogInformation($"Processing CSV file from Blob Storage: {name}");

            try
            {
                using var reader = new StreamReader(blobStream);
                string csvData = await reader.ReadToEndAsync();

                if (await _sftpService.FileExistsAsync($"{_processedFolder}/{name}") ||
                    await _sftpService.FileExistsAsync($"{_failedFolder}/{name}"))
                {
                    _logger.LogWarning($"File {name} already exists in processed or failed folder. Skipping.");
                    return;
                }

                await _sftpService.UploadFileAsync(name, csvData);
                await MoveToArchiveAsync(name, csvData);

                _logger.LogInformation($"File {name} successfully uploaded and archived.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing {name}: {ex.Message}");
            }
        }

        private async Task MoveToArchiveAsync(string fileName, string fileData)
        {
            var archiveBlobClient = _blobServiceClient.GetBlobContainerClient(_archiveCsvContainer).GetBlobClient(fileName);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileData));
            await archiveBlobClient.UploadAsync(stream, overwrite: true);
        }
    }
}
