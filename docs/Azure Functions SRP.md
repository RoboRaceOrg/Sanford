# Keeping Azure Functions with Single Responsibility

## 1. One Function, One Task
- Each **Azure Function** should perform a **single, well-defined task**. Avoid bundling multiple responsibilities into one function.
- Example: Instead of one function handling both **order validation and payment processing**, separate them into:
  - `ValidateOrderFunction`
  - `ProcessPaymentFunction`

## 2. Use Durable Functions for Complex Workflows
- If a function requires **multiple steps**, use **Durable Functions** to orchestrate different tasks.
- Example: Instead of one function handling **user registration, sending confirmation emails, and updating records**, use a **Durable Orchestration Function**.

## 3. Follow Event-Driven Design
- Use **event-based triggers** like **HTTP, Queue, Blob Storage, or Event Grid** to keep functions loosely coupled.
- Example: A function triggered by **an HTTP request** should only **validate input and enqueue a message**, while another function processes the queue.

## 4. Use Dependency Injection for Reusability
- Move shared logic (e.g., logging, database access) into **services** and inject them into functions.
- Example:
  
  ```csharp
  public class OrderValidatorFunction
  {
      private readonly IOrderService _orderService;
      
      public OrderValidatorFunction(IOrderService orderService)
      {
          _orderService = orderService;
      }

      [FunctionName("ValidateOrder")]
      public async Task<IActionResult> Run(HttpRequest req, ILogger log)
      {
          var order = await req.ReadFromJsonAsync<Order>();
          var isValid = _orderService.Validate(order);
          return new OkObjectResult(isValid);
      }
  }
  ```

## 5. Separate Concerns with Queues & Topics
- Instead of **one function handling everything**, break tasks into smaller functions communicating via **Azure Service Bus** or **Storage Queues**.
- Example:
  - `OrderPlacedFunction` → places an order & queues a message
  - `InventoryCheckFunction` → processes the queue & updates stock

## 6. Keep Functions Stateless
- Azure Functions should **not** maintain state. Use **Azure Table Storage, CosmosDB, or Redis** for temporary data persistence.

## 7. Use Naming Conventions & Folder Structure
- Group related functions logically:
  ```
  /OrderFunctions/
      ValidateOrderFunction.cs
      ProcessOrderFunction.cs
  /PaymentFunctions/
      ProcessPaymentFunction.cs
      VerifyPaymentFunction.cs
  ```
- Name functions clearly: `ProcessOrder`, `SendEmailNotification`, etc.

By keeping each function **focused on a single responsibility**, you improve **testability, maintainability, and scalability** of your Azure Functions-based system. 
