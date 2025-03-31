
using Azure.Storage.Blobs;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;


namespace BlueCorpDispatchAuto
{
    public class UploadCsvTimerToSftpFunction
    {
        private readonly ILogger<UploadCsvTimerToSftpFunction> _logger;
        private readonly ISftpService _sftpService;
        private readonly IQueueService _queueService;
        private readonly string _archiveCsvContainer;
        private readonly string _queueName = "processed-csv";
        private readonly string _processedFolder = "bluecorp-processed";
        private readonly string _failedFolder = "bluecorp-failed";
        private readonly BlobServiceClient _blobServiceClient; // Inject BlobServiceClient

        public UploadCsvTimerToSftpFunction(ILogger<UploadCsvTimerToSftpFunction> logger, ISftpService sftpService, IQueueService queueService, BlobServiceClient blobServiceClient)
        {
            _logger = logger;
            _sftpService = sftpService;
            _queueService = queueService;
            _archiveCsvContainer = "dispatch-archive";
            _blobServiceClient = blobServiceClient;
        }

        [Function("UploadCsvTimerToSftp")]
        public async Task RunAsync([TimerTrigger("%SFTP_TIMER_SCHEDULE%")] TimerInfo myTimer)
        {
            DateTime currentTimeUtc = DateTime.UtcNow; // Get current UTC time
            DateTime currentTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(currentTimeUtc, TimeZoneInfo.Local); // Convert to local time

            _logger.LogInformation($"Executing scheduled SFTP upload process at {currentTimeLocal:HH:mm:ss} (Local Time).");

            while (true)
            {
                var message = await _queueService.NextMessageAsync(_queueName);
                if (message == null)
                    break; // No more messages in the queue

                string fileName = message.Name;
                string? csvData = await _queueService.RetrieveMessageAsync(_queueName,fileName);

                // Check if file already exists in processed or failed folder
                if (await _sftpService.FileExistsAsync($"{_processedFolder}/{fileName}") ||
                    await _sftpService.FileExistsAsync($"{_failedFolder}/{fileName}"))
                {
                    _logger.LogWarning($"File {fileName} already exists in processed or failed folder. Skipping and dequeuing.");
                    await _queueService.DequeueMessageAsync(_queueName, fileName);
                    continue;
                }

                try
                {
                    // Upload file to SFTP
                    await _sftpService.UploadFileAsync(fileName, csvData);
                    await MoveToArchiveAsync(fileName, csvData);
                    _logger.LogInformation($"File {fileName} successfully uploaded and archived.");
                    await _queueService.DequeueMessageAsync(_queueName, fileName);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing {fileName}: {ex.Message}");
                }
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
