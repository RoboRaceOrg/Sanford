# **Configuring and Running `UploadCsvTimerToSftpFunction`**

## **Overview**
The `UploadCsvTimerToSftpFunction` is an Azure Function that runs on a scheduled timer to process messages from the `processed-csv` queue and send CSV files via SFTP. It ensures files are not reprocessed by checking the `bluecorp-processed` and `bluecorp-failed` folders on the SFTP server before uploading.

To make the function configurable, the **timer schedule** is stored in an environment variable called `SFTP_TIMER_SCHEDULE`, which allows the schedule to be modified without changing the code.

---

## **Setting Up the Function for Local Development**

### **1. Update `local.settings.json`**
To configure the timer trigger locally, modify the `local.settings.json` file in your Azure Functions project:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "SFTP_TIMER_SCHEDULE": "0 0 18 * * *"
  }
}
```

### **2. Ensure the Function Uses the Environment Variable**
The function should use:
```csharp
[TimerTrigger("%SFTP_TIMER_SCHEDULE%")]
```
This allows the function to read the schedule dynamically from `local.settings.json`.

### **3. Start the Function Locally**
Run the following command in the terminal:
```sh
func start
```
This starts the Azure Function locally and loads the settings from `local.settings.json`.

---

## **Configuring the Function in an Azure Function App**

### **1. Navigate to Azure Function App**
- Go to **Azure Portal** → Select your **Function App**.
- Click on **Configuration** under **Settings**.

### **2. Add the Environment Variable**
- Click **New Application Setting**.
- Set **Name** to `SFTP_TIMER_SCHEDULE`.
- Set **Value** to the desired CRON expression (e.g., `0 0 18 * * *` for 6:00 PM UTC daily).
- Click **OK** and **Save** the configuration.

### **3. Restart the Function App**
After updating the configuration:
- Go to **Azure Function App**.
- Click **Restart** to apply the changes.

---

## **Understanding the CRON Expression**
Azure Timer Triggers use the following format:
```
{second} {minute} {hour} {day} {month} {day-of-week}
```
| Expression  | Meaning |
|-------------|---------|
| `0 0 18 * * *` | Runs every day at **6:00 PM UTC** |
| `0 30 9 * * 1-5` | Runs at **9:30 AM UTC** on **weekdays (Mon-Fri)** |
| `0 */10 * * * *` | Runs **every 10 minutes** |

For local development, make sure to set a schedule that fits your testing needs.

---

## **Final Notes**
- The `SFTP_TIMER_SCHEDULE` value allows flexibility in scheduling the function without modifying the code.
- Azure Portal allows dynamic updates to the schedule via **Application Settings**.
- Local development can use `local.settings.json` to test different schedules.

By following this setup, `UploadCsvTimerToSftpFunction` can run efficiently and be configured dynamically in both **local and cloud environments**.

