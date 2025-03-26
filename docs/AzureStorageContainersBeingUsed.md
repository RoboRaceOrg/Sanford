# **Azure Storage Containers Being Used**

## **1. Identified Blob Storage Containers**

Based on the uploaded files, the following Azure Blob Storage containers are in use:

### **1.1 **``**)**

- **Purpose:** Stores processed CSV files.
- **Usage:**
  - Files are uploaded after processing.
  - Each file is stored in the `processed-csv` container using the method `UploadCsvToBlobAsync()`.

### **1.2 Dynamic Blob Containers (From ****\`\`****)**

- **Purpose:** Stores files dynamically based on the provided container name.
- **Usage:**
  - The method `EnqueueMessageAsync()` uploads files to a container based on the `blobStorageName` parameter.
  - The method `DequeueMessageAsync()` deletes files from a specified container (acting as a queue).

### **1.3 **``**)**

- **Purpose:** Stores raw JSON files.
- **Usage:**
  - JSON data is uploaded using `UploadJsonAsync()`.
  - JSON files can be retrieved using `DownloadJsonAsync()`.
  - Existence of a file is checked using `JsonExistsAsync()`.

  ### **1.4 **`dispatch-archive`**)**
- 
- - **Purpose:** Stores archieved csv files.
- **Usage:**
  - Csv data is uploaded using `UploadCsvToSftpFunction`.
  - Csv files is uploaded using `UploadCsvTimerToSftpFunction`.
  - Existence of a file is checked using `JsonExistsAsync()`.
  
---

## **2. How to Set Up These Containers in Azure and Locally**

### **2.1 Setting Up Containers in Azure**

You can create blob containers using **Azure Portal**, **Azure CLI**, or **C# SDK**.

#### **Using Azure CLI:**

```sh
# Login to Azure
az login

# Create a storage account
az storage account create --name mystorageaccount --resource-group myResourceGroup --location eastus --sku Standard_LRS

# Create blob containers
az storage container create --name processed-csv --account-name mystorageaccount
az storage container create --name raw-json --account-name mystorageaccount
```

#### **Using C# SDK:**

```csharp
var blobServiceClient = new BlobServiceClient(connectionString);
await blobServiceClient.GetBlobContainerClient("processed-csv").CreateIfNotExistsAsync();
await blobServiceClient.GetBlobContainerClient("raw-json").CreateIfNotExistsAsync();
```

---

### **2.2 Setting Up Containers Locally**

For local development, **Azure Storage Emulator** or **Azurite** can be used.

#### **Option 1: Using Azurite (Recommended)**

- Install Azurite (local storage emulator):
  ```sh
  npm install -g azurite
  ```
- Start Azurite:
  ```sh
  azurite
  ```
- Use the local storage connection string:
  ```sh
  "UseDevelopmentStorage=true"
  ```

#### **Option 2: Using Azure CLI for Local Storage**

```sh
az storage container create --name processed-csv --account-name devstorage --connection-string UseDevelopmentStorage=true
az storage container create --name raw-json --account-name devstorage --connection-string UseDevelopmentStorage=true
```

---

## **3. Configuration in Azure Function**

Modify `local.settings.json` to include local blob storage settings:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "ProcessedCsvContainer": "processed-csv",
    "RawJsonContainer": "raw-json"
  }
}
```

For **production**, set the `AzureWebJobsStorage` in **Azure App Configuration** with the real Azure Storage connection string.

---

By following these steps, you can effectively manage Azure Storage containers for development and production environments.

