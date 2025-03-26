# Azure API Management (APIM) Setup for Forwarding JSON to SaveJsonToBlobStore

## 1. Overview
This document provides a step-by-step guide to configuring **Azure API Management (APIM)** to forward JSON payloads to the **SaveJsonToBlobStore** Azure Function. The setup ensures secure and reliable processing of JSON data within Azure.

---

## 2. Create an API in Azure API Management
1. Navigate to **Azure API Management** in the **Azure Portal**.
2. Select **APIs** and click **+ Add API**.
3. Choose **HTTP** as the API type.
4. Set the **Base URL** to match the **SaveJsonToBlobStore** Azure Function endpoint.

---

## 3. Define API Endpoints
1. Create an **operation** with the following details:
   - **Method:** `POST`
   - **Path:** `/dispatch/json`
   - **Request Payload:** JSON format
2. Configure the backend settings to route requests to the **SaveJsonToBlobStore** function.

---

## 4. Secure API with OAuth 2.0
1. Enable **OAuth 2.0 client credentials** for authentication.
2. Register an **Azure AD Application** to issue OAuth tokens.
3. Configure APIM to enforce OAuth authentication before forwarding requests.

---

## 5. Implement Policy for Forwarding Requests
1. Navigate to **Azure API Management Policies**.
2. Add the following **inbound policy** to forward requests securely:

```xml
<inbound>
    <base />
    <authentication-managed-identity resource="https://management.azure.com/" />
    <set-header name="x-functions-key" exists-action="override">
        <value>{{function-key}}</value>
    </set-header>
    <set-backend-service base-url="https://yourfunctionapp.azurewebsites.net/api/SaveJsonToBlobStore" />
</inbound>
```

3. Ensure that **SaveJsonToBlobStore** function requires an API key or Azure AD authentication.

---

## 6. Configure Backend Integration
1. Verify APIM can communicate with the **SaveJsonToBlobStore** function.
2. If APIM is in a **VNet**, enable **VNet integration**.
3. If calling a **private function**, configure **Azure Private Link**.

---

## 7. Testing the Integration
1. Use **Postman** or APIM’s test feature to send a **POST** request with a JSON payload.
2. Check if the **SaveJsonToBlobStore** function processes the JSON and saves it to **Azure Blob Storage**.
3. Monitor logs in **Application Insights** for troubleshooting.

---

## 8. Summary
This setup enables APIM to securely forward JSON data from external sources (e.g., D365) to an Azure Function for storage in **Azure Blob Storage**. It ensures:
- Secure authentication using OAuth 2.0 or function keys.
- Scalable and reliable request handling.
- Flexible integration with **Azure Private Link** or **VNet**.

---

**Next Steps:**
- Implement error handling in **APIM Policies**.
- Enable logging for auditing requests.
- Set up monitoring for **API performance** and **latency tracking**.

