using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BlueCorpDispatchAuto;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Determine environment (default to Development if not set)
var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";

// Get the Blob Storage connection string from environment variables.
var blobConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

if (string.IsNullOrEmpty(blobConnectionString))
{
    throw new InvalidOperationException("AzureWebJobsStorage environment variable is not set.");
}

// Load configuration from appropriate sources
builder.Configuration
    .AddJsonFile("local.settings.json", optional: environment == "Development", reloadOnChange: true) // Local dev only
    .AddEnvironmentVariables(); // Always loads environment variables

// Bind SFTP settings from configuration
builder.Services.Configure<SftpOptions>(builder.Configuration.GetSection("SftpSettings"));
// Register SftpService as a singleton
builder.Services.AddSingleton<ISftpService, SftpService>();

// Register BlobServiceClient.
builder.Services.AddSingleton(x => new BlobServiceClient(blobConnectionString));

// Register configuration for BlobStorageServiceOptions
builder.Services.Configure<BlobStorageServiceOptions>(options =>
{
    // Set the container name from environment variable, or default to "processed-csv"
    options.ProcessedCsvContainer = Environment.GetEnvironmentVariable("ProcessedCsvContainer") ?? "processed-csv";
});

// Register BlobStorageService.
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

// Register configuration for RawJsonBlobStorageServiceOptions
builder.Services.Configure<RawJsonBlobStorageServiceOptions>(options =>
{
    options.RawJsonContainer = Environment.GetEnvironmentVariable("RawJsonContainer") ?? "raw-json";
});

// Register RawJsonBlobStorageService.
builder.Services.AddScoped<IRawJsonBlobStorageService, RawJsonBlobStorageService>();

builder.Services.AddSingleton<IQueueService, QueueService>();
builder.Services.AddTransient<IBlobStorageService, BlobStorageService>();

await CreateBlobContainers(builder);

PreFlightCheck();

builder.Build().Run();





async Task CreateBlobContainers(FunctionsApplicationBuilder builder)
{
    var serviceProvider = builder.Services.BuildServiceProvider();
    var blobServiceClient = serviceProvider.GetRequiredService<BlobServiceClient>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        // Example: Create "raw-json" container
        var rawJsonContainerClient = blobServiceClient.GetBlobContainerClient("raw-json");
        await rawJsonContainerClient.CreateIfNotExistsAsync(PublicAccessType.None);
        logger.LogInformation("raw-json container created or already exists.");

        // Example: Create "processed-csv" container
        var processedCsvContainerClient = blobServiceClient.GetBlobContainerClient("processed-csv");
        await processedCsvContainerClient.CreateIfNotExistsAsync(PublicAccessType.None);
        logger.LogInformation("processed-csv container created or already exists.");

        // Example: Create "dispatch-archive" container
        var archiveContainerClient = blobServiceClient.GetBlobContainerClient("dispatch-archive");
        await archiveContainerClient.CreateIfNotExistsAsync(PublicAccessType.None);
        logger.LogInformation("dispatch-archive container created or already exists.");

        // Add other container creation logic here...
    }
    catch (Exception ex)
    {
        logger.LogError($"Error creating Blob containers: {ex.Message}");
    }
}

void  PreFlightCheck()
{
    // List of required environment variables
    var requiredVariables = new List<string> { "SftpSettings:PrivateKeyPath", "AzureWebJobsStorage", "FUNCTIONS_WORKER_RUNTIME", "SftpSettings:Host", 
        "SftpSettings:Username", "SftpSettings:RemoteFolder" };

    // Call the reusable method
    var result = EnvironmentVariableChecker.CheckEnvironmentVariables(requiredVariables);

    // Display the result
    Console.WriteLine($"Status: {result.Status}");
    Console.WriteLine($"Message: {result.Message}");
}