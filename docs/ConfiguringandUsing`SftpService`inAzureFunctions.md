# **Configuring and Using `SftpService` in Azure Functions**

## **Overview**
The `SftpService` is responsible for **uploading files via SFTP** and **checking if files exist in remote directories** (`bluecorp-processed` and `bluecorp-failed`). The service is **registered as a singleton** in the `Program.cs` file, ensuring that a single instance is used throughout the application.

---


## **1. Registering `SftpService` as a Singleton in `Program.cs`**

In **Azure Function Apps**, services are registered in the **Program.cs** file. To make `SftpService` a **singleton**, register it in **dependency injection (DI)** as follows:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BlueCorpDispatchAuto;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((hostContext, services) =>
    {
        // Bind SFTP settings from configuration
        services.Configure<SftpOptions>(hostContext.Configuration.GetSection("SftpSettings"));

        // Register SftpService as a singleton
        services.AddSingleton<ISftpService, SftpService>();
    })
    .Build();

host.Run();
```

### **How This Works:**
- **`Configure<SftpOptions>`** binds the SFTP settings from **Azure Configuration** or `local.settings.json`.
- **`AddSingleton<ISftpService, SftpService>()`** ensures a single instance of `SftpService` is created and used across the application.

---

## **2. Configuring SFTP Settings in `local.settings.json` (Local Development)**
To run the service locally, add the **SFTP configuration** to `local.settings.json`:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "SftpSettings:Host": "sftp.example.com",
    "SftpSettings:Username": "sftp-user",
    "SftpSettings:PrivateKeyPath": "C:\\keys\\id_rsa",
    "SftpSettings:RemoteFolder": "/uploads"
  }
}
```

---

## **3. Configuring SFTP in Azure Function App (Cloud Deployment)**
In **Azure Portal**, follow these steps to configure the environment variables:

1. **Go to Azure Portal** → Navigate to your **Function App**.
2. Click on **Configuration** under **Settings**.
3. Under **Application settings**, click **New application setting** and add the following:
   - `SftpSettings:Host` → **sftp.example.com**
   - `SftpSettings:Username` → **sftp-user**
   - `SftpSettings:PrivateKeyPath` → **/home/site/wwwroot/id_rsa**
   - `SftpSettings:RemoteFolder` → **/uploads**
4. Click **Save** and **Restart** the Function App.

---

## **4. Using `SftpService` in an Azure Function**
Now that the `SftpService` is injected as a **singleton**, it can be used in **any function** that requires SFTP operations. For example, in `UploadCsvToSftpFunction`:

```csharp
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BlueCorpDispatchAuto
{
    public class UploadCsvToSftpFunction
    {
        private readonly ISftpService _sftpService;

        public UploadCsvToSftpFunction(ISftpService sftpService)
        {
            _sftpService = sftpService;
        }

        [Function("UploadCsvToSftp")]
        public async Task RunAsync([BlobTrigger("processed-csv/{name}", Connection = "AzureWebJobsStorage")] Stream blobStream, string name, ILogger log)
        {
            log.LogInformation($"Processing CSV file: {name}");

            try
            {
                using var reader = new StreamReader(blobStream);
                string csvData = await reader.ReadToEndAsync();

                await _sftpService.UploadFileAsync(name, csvData);
                log.LogInformation($"File {name} successfully uploaded to SFTP.");
            }
            catch (Exception ex)
            {
                log.LogError($"Error processing {name}: {ex.Message}");
            }
        }
    }
}
```

---

## **Final Notes**
- **`SftpService` is a singleton**, meaning it is created **once** and used throughout the application.
- **Configuration is managed via environment variables** (local or cloud-based).
- **It supports both local and Azure deployments**, with configurations stored in **local.settings.json** for development and in **Azure Function App Configuration** for production.
- **Ensures reusability and maintainability**, allowing any function to use **SFTP operations** without reinitialization.

By following this approach, `SftpService` is efficiently managed and integrated into the **Azure Function App**.

