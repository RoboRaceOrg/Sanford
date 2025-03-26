
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BlueCorpDispatchAuto

{

        public class ConvertJsonToCsvFunction
        {
            private readonly ILogger<ConvertJsonToCsvFunction> _logger;
            private readonly IQueueService _queueService;

            public ConvertJsonToCsvFunction(ILogger<ConvertJsonToCsvFunction> logger, IQueueService queueService)
            {
                _logger = logger;
                _queueService = queueService;
            }

            [Function("ConvertJsonToCsv")]
            public async Task Run(
                [BlobTrigger("%RawJsonContainer%/{name}", Connection = "AzureWebJobsStorage")] Stream blobStream,
                string name)
            {
                _logger.LogInformation($"Processing JSON: {name}");

                try
                {
                    using var reader = new StreamReader(blobStream);
                    string jsonContent = await reader.ReadToEndAsync();

                    string csvData = JsonToCsvConverter.ConvertJsonToCsv(jsonContent);
                    string csvFileName = name.Replace(".json", ".csv");

                    await _queueService.EnqueueMessageAsync("processed-csv", csvFileName, csvData);
                    _logger.LogInformation($"CSV {csvFileName} added to queue successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing JSON: {ex.Message}");
                }
            }
        }
    }