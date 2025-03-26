
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using BlueCorpDispatchAuto;

public class SaveJsonToBlobStoreFunction
{
    private readonly ILogger<SaveJsonToBlobStoreFunction> _logger;
    private readonly IRawJsonBlobStorageService _blobStorageService;

    public SaveJsonToBlobStoreFunction(ILogger<SaveJsonToBlobStoreFunction> logger, IRawJsonBlobStorageService blobStorageService)
    {
        _logger = logger;
        _blobStorageService = blobStorageService;
    }

    [Function("SaveJsonToBlobStore")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("Received payload from D365.");

        try
        {
            using var reader = new StreamReader(req.Body);
            string requestBody = await reader.ReadToEndAsync();
            var missingFields = BlueCorpDispatchRequestValidator.ValidateJsonData(requestBody);

            if (missingFields.Count > 0)
            {
                var validationError = new ObjectResult(new { error = "Validation Failed", missingFields })
                {
                    StatusCode = StatusCodes.Status422UnprocessableEntity
                };
                return validationError;
            }

            var jsonDocument = JsonDocument.Parse(requestBody);
            string blobName = $"dispatch_{jsonDocument.RootElement.GetProperty("controlNumber").GetUInt64()}_{jsonDocument.RootElement.GetProperty("salesOrder").GetString()}.json";

            if (!await _blobStorageService.JsonExistsAsync(blobName))
            {
                await _blobStorageService.UploadJsonAsync(blobName, requestBody);
                return new OkObjectResult($"JSON file {blobName} stored successfully.");
            }
            else
            {
                _logger.LogWarning($"JSON file {blobName} already exists in Azure Blob Storage.");
                return new ConflictObjectResult($"JSON file {blobName} already exists.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing request: {ex.Message}");
            var internalError = new ObjectResult("Internal Server Error")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            return internalError;
        }
    }
}
