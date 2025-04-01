# **BlueCorp Dispatch Request Automation Solution**

## **Requirements Analysis**

### **Functional Requirements**
1. **Automate Data Flow from D365 to 3PL:**
   - Capture JSON payload from D365 when the "Ready for Dispatch" button is clicked.
   - Convert the JSON payload to the required CSV format.
   - Upload the CSV file to the 3PL SFTP site.

2. **Data Mapping:**
   - Transform JSON fields into CSV fields based on the mapping specifications.

3. **Authentication & Security:**
   - Support OAuth 2.0 client credentials for D365 integration.
   - Use Public Key Authentication for SFTP file transfer.
   - Ensure data encryption at rest and in transit.

4. **Reliability & Error Handling:**
   - Implement automatic retries for transient failures (e.g., SFTP site temporarily unavailable).
   - Log and monitor failures, and provide alerts.

5. **Monitoring & Logging:**
   - Capture integration telemetry in Azure Monitor.

### **Non-Functional Requirements**
- **Use Azure Integration Services.**
- **Deploy automation using Azure DevOps Pipelines.**
- **Ensure compliance with BlueCorp's architecture standards.**
- **Ensure outbound SFTP traffic complies with 3PL's security requirements.**

---

## **Proposed Azure-Based Solution**

### **Step 1: D365 to Azure API Management (APIM)**
- D365 sends an **HTTP POST** request when the "Ready for Dispatch" button is clicked.
- The request is sent to **Azure API Management (APIM)**, which acts as a secure gateway.
- APIM authenticates the request using **OAuth 2.0 client credentials** and forwards it to an **Azure Function App** that processes and stores the JSON in Blob Storage.

### **Step 2: Azure Function (HTTP Trigger) - Store JSON in Azure Blob Storage**
- Receives the JSON payload from **APIM**.
- Validates the request against the schema.
- Stores the JSON in **Azure Blob Storage** under the **raw-json** container.

### **Step 3: Azure Function (Blob Trigger) - Convert JSON to CSV**
- Triggered when a new JSON file is added or updated in **raw-json**.
- Reads the JSON, transforms it into **CSV format**, and saves the CSV to **Processed CSV Container**.

### **Step 4: Azure Function (Blob Trigger) - Upload CSV to 3PL via SFTP**
- Triggered when a new CSV file is added to **Processed CSV Container**.
- Uploads the CSV file to the **3PL SFTP site** using **Public Key Authentication**.
- Moves successfully uploaded files to **Dispatch Archive Container**.

### **Step 5: Monitoring & Logging**
- **Azure Monitor and Application Insights** track system performance and failures.
- **Azure Monitor Alerts** notify the support team in case of failures.

---

## **Technology Stack**

| Component | Service Used | Purpose |
|-----------|-------------|---------|
| API Gateway | **Azure API Management (APIM)** | Secures and manages D365 API requests |
| Compute | **Azure Function App** | Handles data processing, transformation, and SFTP upload |
| Storage | **Azure Blob Storage** | Temporary storage for JSON and CSV files |
| Secure File Transfer | **Azure Functions using SFTP** | Uploads CSV files to 3PL |
| Logging & Monitoring | **Azure Monitor, Application Insights** | Tracks system performance and failures |
| CI/CD | **Azure DevOps Pipelines** | Automates build and deployment |

---

## **Implementation Steps**

### **Step 1: Configure D365 to Send Data**
- Register an API endpoint in **Azure API Management** to receive the JSON payload.
- Configure D365 to send HTTP POST requests to APIM.
- **Configure Azure API Management to forward the request to the Azure Function in Step 2**.
- Register an API endpoint in **Azure API Management** to receive the JSON payload.
- Configure D365 to send HTTP POST requests to APIM.

### **Step 2: Deploy Azure Function for JSON Storage**
- **Function:** `SaveJsonToBlobStore`
- **Trigger:** HTTP (POST request from APIM)
- **Task:** Stores JSON in Azure Blob Storage (`raw-json` container).

### **Step 3: Deploy Azure Function for JSON to CSV Conversion**
- **Function:** `ConvertJsonToCsv`
- **Trigger:** Blob Storage Trigger (new file in `raw-json` container)
- **Task:** Transforms JSON data into CSV format and stores it in `processed-csv` container.

### **Step 4: Deploy Azure Function for SFTP Upload**
- **Function:** `UploadCsvToSftp`
- **Trigger:** Blob Storage Trigger (new file in `processed-csv` container)
- **Task:** Uploads CSV to **3PL SFTP server** and moves processed files to **dispatch-archive**.

### **Step 5: Implement Monitoring & Error Handling**
- Configure **retry policies** in Azure Functions.
- Log errors in **Application Insights**.
- Set up **Azure Monitor alerts** for failures.

---

## **Final Solution Flow**

1. **D365 sends JSON → Azure API Management**.
2. **APIM forwards JSON → Azure Function stores in Blob Storage (`raw-json`).**
3. **Blob Trigger fires → Azure Function converts JSON → CSV → Stores in `processed-csv`**.
4. **Blob Trigger fires → Azure Function uploads CSV to 3PL SFTP → Archives processed files.**
5. **Azure Monitor & Logging track failures → Alerts support team as needed.**

This architecture **fully automates** BlueCorp’s dispatch request process while **ensuring security, reliability, and compliance with Azure best practices**.

---

# **3PL IP Whitelisting Compliance in BlueCorp Dispatch Automation**

## **Meeting the 3PL IP Whitelisting Requirement**

### **Requirement**
The **3PL provider** requires all companies integrating via **SFTP** to provide a **static IP address** that will be whitelisted in their firewall to allow secure connections.

### **How the Proposed Solution Meets This Requirement**

### **1. Azure Virtual Network (VNet) with a Static Public IP**
- The **Azure Function App** running `UploadCsvToSftpFunction` is integrated with an **Azure Virtual Network (VNet)`.
- A **NAT Gateway** or **Azure Firewall** is configured with a **Static Public IP** to ensure all outbound traffic (including SFTP) originates from a single whitelisted IP.

### **2. Deploying Azure Functions in a Premium or Dedicated App Service Plan**
- The Azure Function App must be deployed on a **Premium (EP1 or higher) or Dedicated App Service Plan**.
- This deployment model supports **VNet Integration**, which is necessary for assigning a **static outbound IP**.
- **Consumption plans are not supported** because they do not guarantee a static outbound IP.

### **3. Configuring the 3PL Firewall with the Static IP**
- The **static IP assigned to the NAT Gateway** is provided to the **3PL provider** for whitelisting in their firewall.
- This ensures that **only authorized traffic** from BlueCorp’s system can access the **3PL SFTP server**.

This approach ensures that **all SFTP requests originate from a consistent, whitelisted IP**, fully meeting the **3PL’s security requirement**.

---
## **Links to Further Documentation**

[Azure API Management (APIM) Setup for Forwarding JSON to SaveJsonToBlobStore](docs/AzureAPIManagement(APIM)SetupforForwardingJSONtoSaveJsonToBlobStore.md)

[xUnit Test Cases Summary](docs/xUnitTestCasesSummary.md)

[Setting Up Environments](docs/Setting_Up_Environments.md)

[Configuring and Running UploadCsvTimerToSftpFunction](docs/ConfiguringandRunning`UploadCsvTimerToSftpFunction`.md)

[Configuring and Using `SftpService` in Azure Functions](docs/ConfiguringandUsing`SftpService`inAzureFunctions.md)

[Azure Storage Containers Being Used](docs/AzureStorageContainersBeingUsed.md)

[Why My Azure Functions Project Skips Azure Key Vault](docs/WhyMyAzureFunctionsProjectSkipsAzureKeyVault.md)

---
## **To Do**
- Create build pipelines for Azure deployment
- Create ARM or Bicep scipts to automate azure resource creation for different enviroments such as Dev, Test and Production
- Configure VNet in Azure to meet static IP requirements and for whitelisting requests
- Configure Azure API Management for Dev, Test and Production
- Further testing of Sftp azure functions when sFTP connectivity fails, or 3pl sftp is not available.
- imporve Unit testing to cover more edge cases.
- Setup Azure DevOps to go through an Approval process for final deployment into environments such as Test and Production.