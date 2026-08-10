# BuildingBlock.Saga

Enterprise-grade Saga Orchestration pattern implementation for distributed transactions in microservices architecture.

## Overview

The Saga pattern solves distributed transactions by breaking them into a series of local transactions coordinated by a saga orchestrator. Each step can execute a compensating transaction to rollback if a later step fails.

**Use cases:**
- Order creation (Payment → Inventory → Fulfillment)
- User registration workflows (Auth → Profile → Email Notification)
- Refund processing (Reverse Payment → Restore Inventory)
- Multi-service operations requiring ACID-like guarantees

## Architecture

```
┌─────────────────────────────────────────────┐
│         SagaOrchestrator                    │
│  - Executes steps in order                  │
│  - Handles compensation on failure          │
│  - Logs execution and failures              │
└────────┬────────────────────────────────────┘
         │
         ├─ Step 1 (Execute/Compensate)
         ├─ Step 2 (Execute/Compensate)
         ├─ Step 3 (Execute/Compensate)
         └─ Step N (Execute/Compensate)
         │
         └─ ISagaStore (Persistence)
            - InMemorySagaStore (Dev/Test)
            - DatabaseSagaStore (Production)
```

## Core Concepts

- **ISagaDefinition** - Defines the workflow (steps and order)
- **ISagaStep** - Single action with execute/compensate
- **ISagaContext** - Shares data across all steps
- **ISagaOrchestrator** - Executes the workflow with automatic rollback
- **ISagaStore** - Persists saga state for reliability

## Installation

```csharp
// In DependencyInjection.cs
services.AddSagaOrchestration();
// or for production with custom store:
services.AddSagaOrchestration<DatabaseSagaStore>();
```

## Usage Example: Order Creation Saga

### 1. Define Saga Steps

```csharp
public class ReserveInventoryStep : ISagaStep
{
    private readonly IInventoryService _inventory;

    public string StepName => "ReserveInventory";

    public ReserveInventoryStep(IInventoryService inventory)
    {
        _inventory = inventory;
    }

    public async Task ExecuteAsync(ISagaContext context, CancellationToken ct)
    {
        var orderId = context.Get<string>("OrderId")!;
        var items = context.Get<List<OrderItem>>("Items")!;

        // Reserve inventory and save reservation ID
        var reservationId = await _inventory.ReserveAsync(items, ct);
        context.Set("ReservationId", reservationId);
    }

    public async Task CompensateAsync(ISagaContext context, CancellationToken ct)
    {
        var reservationId = context.Get<string>("ReservationId");
        if (reservationId != null)
            await _inventory.ReleaseReservationAsync(reservationId, ct);
    }
}

public class ProcessPaymentStep : ISagaStep
{
    private readonly IPaymentService _payment;

    public string StepName => "ProcessPayment";

    public ProcessPaymentStep(IPaymentService payment)
    {
        _payment = payment;
    }

    public async Task ExecuteAsync(ISagaContext context, CancellationToken ct)
    {
        var orderId = context.Get<string>("OrderId")!;
        var amount = context.Get<decimal>("Amount")!;
        var customerId = context.Get<string>("CustomerId")!;

        // Process payment and save transaction ID
        var transactionId = await _payment.ChargeAsync(customerId, amount, ct);
        context.Set("TransactionId", transactionId);
    }

    public async Task CompensateAsync(ISagaContext context, CancellationToken ct)
    {
        var transactionId = context.Get<string>("TransactionId");
        if (transactionId != null)
            await _payment.RefundAsync(transactionId, ct);
    }
}

public class CreateOrderStep : ISagaStep
{
    private readonly IOrderService _order;

    public string StepName => "CreateOrder";

    public CreateOrderStep(IOrderService order)
    {
        _order = order;
    }

    public async Task ExecuteAsync(ISagaContext context, CancellationToken ct)
    {
        var customerId = context.Get<string>("CustomerId")!;
        var items = context.Get<List<OrderItem>>("Items")!;
        var amount = context.Get<decimal>("Amount")!;

        // Create the order
        var orderId = await _order.CreateAsync(
            customerId,
            items,
            amount,
            ct);

        context.Set("OrderId", orderId);
    }

    public async Task CompensateAsync(ISagaContext context, CancellationToken ct)
    {
        var orderId = context.Get<string>("OrderId");
        if (orderId != null)
            await _order.CancelAsync(orderId, ct);
    }
}
```

### 2. Define the Saga Workflow

```csharp
public class OrderCreationSaga
{
    private readonly ISagaOrchestrator _orchestrator;
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    private readonly IInventoryService _inventoryService;

    public OrderCreationSaga(
        ISagaOrchestrator orchestrator,
        IOrderService orderService,
        IPaymentService paymentService,
        IInventoryService inventoryService)
    {
        _orchestrator = orchestrator;
        _orderService = orderService;
        _paymentService = paymentService;
        _inventoryService = inventoryService;
    }

    public async Task<string> ExecuteAsync(
        string customerId,
        List<OrderItem> items,
        decimal amount,
        CancellationToken ct = default)
    {
        // Build the saga definition
        var sagaDefinition = new SagaDefinitionBuilder("OrderCreationSaga")
            .Description("Create order with inventory reservation and payment processing")
            .AddStep(new ReserveInventoryStep(_inventoryService))
            .AddStep(new ProcessPaymentStep(_paymentService))
            .AddStep(new CreateOrderStep(_orderService))
            .Build();

        // Create saga context with input data
        var context = new SagaContext(
            sagaId: Guid.NewGuid().ToString(),
            correlationId: Guid.NewGuid().ToString(),
            initialData: new Dictionary<string, object?>
            {
                { "CustomerId", customerId },
                { "Items", items },
                { "Amount", amount }
            });

        // Execute the saga
        try
        {
            var result = await _orchestrator.ExecuteAsync(sagaDefinition, context, ct);
            return context.Get<string>("OrderId")!;
        }
        catch (SagaExecutionException ex)
        {
            // Saga failed, compensation was automatic
            // All completed steps were rolled back
            throw;
        }
    }
}
```

### 3. Use the Saga

```csharp
var saga = new OrderCreationSaga(
    orchestrator,
    orderService,
    paymentService,
    inventoryService);

try
{
    var orderId = await saga.ExecuteAsync(
        customerId: "customer-123",
        items: new List<OrderItem> { /* ... */ },
        amount: 99.99m);

    Console.WriteLine($"Order created: {orderId}");
}
catch (SagaExecutionException ex)
{
    Console.WriteLine($"Order creation failed at {ex.FailedStepName}: {ex.Message}");
    // Order, inventory reservation, and payment were all rolled back
}
```

## Step Execution Flow

```
1. Reserve Inventory → Success
   ├─ Step completed
   └─ Continue to next step

2. Process Payment → Success
   ├─ Step completed
   └─ Continue to next step

3. Create Order → FAILURE ❌
   └─ Compensation triggered (reverse order)
      │
      ├─ Compensate Create Order (Cancel order)
      ├─ Compensate Process Payment (Refund)
      └─ Compensate Reserve Inventory (Release reservation)

Result: All changes rolled back, saga in Compensated state
```

## Data Sharing Between Steps

```csharp
// In step 1: Set data
context.Set("ReservationId", reservationId);
context.Set("Amount", 99.99m);

// In step 2: Retrieve data
var reservationId = context.Get<string>("ReservationId");
var amount = context.Get<decimal>("Amount");

// Get all context data
var allData = context.GetAllData();
```

## Saga State Management

States: `Pending → Running → Succeeded/Failed → Compensating → Compensated`

```csharp
context.State; // Current state

// Track completed steps for compensation
context.MarkStepCompleted("ReserveInventory");
var completed = context.GetCompletedSteps(); // ["ReserveInventory", "ProcessPayment"]
```

## Persistence and Recovery

### Development (In-Memory)
```csharp
services.AddSagaOrchestration();
```

### Production (Database)
```csharp
public class DatabaseSagaStore : ISagaStore
{
    private readonly IDbContext _db;

    public async Task SaveAsync(SagaExecutionRecord record, CancellationToken ct)
    {
        // Implement database persistence
        _db.SagaRecords.Add(record);
        await _db.SaveChangesAsync(ct);
    }

    // ... implement other methods
}

services.AddSagaOrchestration<DatabaseSagaStore>();
```

### Query Failed Sagas
```csharp
var failedSagas = await _sagaStore.GetFailedSagasAsync(cancellationToken);
foreach (var saga in failedSagas)
{
    Console.WriteLine($"Saga {saga.SagaId} failed: {saga.FailureReason}");
}
```

## Error Handling

### In Execute Steps
```csharp
public async Task ExecuteAsync(ISagaContext context, CancellationToken ct)
{
    try
    {
        // May throw any exception
        await _service.ProcessAsync(context, ct);
    }
    catch (ServiceException ex)
    {
        // Re-throw or wrap
        throw new InvalidOperationException("Processing failed", ex);
    }
}
```

### In Compensate Steps
```csharp
public async Task CompensateAsync(ISagaContext context, CancellationToken ct)
{
    try
    {
        var id = context.Get<string>("ResourceId");
        if (id != null)
            await _service.DeleteAsync(id, ct);
    }
    catch (Exception ex)
    {
        // Log but DON'T throw
        // Orchestrator catches compensation errors and continues with next step
        _logger.LogError(ex, "Compensation failed");
    }
}
```

## Testing Sagas

```csharp
[Test]
public async Task OrderCreationSaga_ShouldRollbackOnPaymentFailure()
{
    // Arrange
    var inventoryMock = new Mock<IInventoryService>();
    var paymentMock = new Mock<IPaymentService>();
    var orderMock = new Mock<IOrderService>();

    inventoryMock
        .Setup(x => x.ReserveAsync(It.IsAny<List<OrderItem>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync("reservation-123");

    paymentMock
        .Setup(x => x.ChargeAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new PaymentFailedException("Declined"));

    var orchestrator = new SagaOrchestrator(
        new Logger<SagaOrchestrator>(new LoggerFactory()),
        new InMemorySagaStore());

    var saga = new OrderCreationSaga(
        orchestrator,
        orderMock.Object,
        paymentMock.Object,
        inventoryMock.Object);

    // Act & Assert
    var ex = Assert.ThrowsAsync<SagaExecutionException>(
        () => saga.ExecuteAsync("customer-123", items, 99.99m));

    Assert.AreEqual("ProcessPayment", ex.FailedStepName);

    // Verify compensation was called
    inventoryMock.Verify(
        x => x.ReleaseReservationAsync("reservation-123", It.IsAny<CancellationToken>()),
        Times.Once);
}
```

## Best Practices

1. **Keep steps small** - Each step should do one thing
2. **Idempotent compensation** - Compensation should be safe to call multiple times
3. **Avoid long-running steps** - Use timeouts and async patterns
4. **Log everything** - Track saga execution for debugging
5. **Use correlation IDs** - Link related operations across services
6. **Implement persistence** - Use database store for production reliability
7. **Monitor saga metrics** - Track success rates, execution times, rollbacks
8. **Compensate gracefully** - Log but don't fail compensation due to service unavailability

## Integration with MessageQueue

Combine with `BuildingBlock.MessageQueue` for event-driven sagas:

```csharp
public class PublishOrderCreatedEventStep : ISagaStep
{
    private readonly IMessageProducer _producer;

    public string StepName => "PublishOrderCreated";

    public async Task ExecuteAsync(ISagaContext context, CancellationToken ct)
    {
        var orderId = context.Get<string>("OrderId")!;

        await _producer.PublishAsync(
            topic: "order.created",
            message: new OrderCreatedEvent { OrderId = orderId },
            key: orderId,
            cancellationToken: ct);
    }

    public Task CompensateAsync(ISagaContext context, CancellationToken ct) => Task.CompletedTask;
}
```

## Performance Tips

- Steps execute sequentially by design (ensures consistency)
- Use async/await throughout for non-blocking I/O
- Cache frequently accessed data in context
- Set appropriate timeouts on compensation steps
- Monitor saga execution times and adjust step order if needed

## Common Patterns

### Conditional Steps (If-Then Logic)
```csharp
public override async Task ExecuteAsync(ISagaContext context, CancellationToken ct)
{
    var amount = context.Get<decimal>("Amount");
    
    if (amount > 1000)
    {
        // Require approval for large orders
        var approved = await _approvalService.RequestAsync(amount, ct);
        if (!approved)
            throw new InvalidOperationException("Order requires approval");
    }
}
```

### Parallel Sagas
```csharp
// Execute multiple sagas in parallel
var saga1 = Task.Run(() => orderSaga.ExecuteAsync(data1));
var saga2 = Task.Run(() => reportSaga.ExecuteAsync(data2));

await Task.WhenAll(saga1, saga2);
```

### Saga Chaining
```csharp
// One saga triggers another
var orderCreationResult = await orderSaga.ExecuteAsync(data);
var fulfillmentResult = await fulfillmentSaga.ExecuteAsync(orderCreationResult);
```
