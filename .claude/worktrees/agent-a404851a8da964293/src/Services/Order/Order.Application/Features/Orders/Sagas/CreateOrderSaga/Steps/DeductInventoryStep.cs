using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Contract.Events.Order;
using NovaCore.BuildingBlock.Saga.Abstractions;

using NovaCore.Order.Application.Abstractions.Services;

namespace NovaCore.Order.Application.Features.Orders.Sagas.CreateOrderSaga.Steps;

/// <summary>
/// Saga Step 1 (see PHASE 6 of the CreateOrder workflow). Calls Inventory's gRPC DeductStock,
/// keyed by OrderId for idempotency - a saga restart/redelivery re-executes this safely without
/// double-deducting (see Inventory's StockDeduction ledger). A business failure (insufficient
/// stock) throws InventoryDeductionFailedException rather than returning a sentinel, so
/// SagaOrchestrator's existing fail/compensate/rethrow path does the work - the saga has nothing
/// completed yet to compensate at this point, so compensation here is a no-op and control returns
/// to OrderCreatedSagaConsumer's catch block to cancel the order.
/// </summary>
public sealed class DeductInventoryStep(
    IInventoryClientService inventoryClient,
    IAppLogger<DeductInventoryStep> logger) : ISagaStep
{
    public const string Name = "DeductInventory";
    public string StepName => Name;

    public async Task ExecuteAsync(ISagaContext context, CancellationToken ct = default)
    {
        var orderId = context.Get<Guid>(CreateOrderSagaContextKeys.OrderId);
        var items = context.Get<IReadOnlyCollection<OrderCreatedItem>>(CreateOrderSagaContextKeys.Items)
            ?? throw new InvalidOperationException("CreateOrderSaga context is missing Items.");

        var deductionItems = items
            .Select(i => new InventoryDeductionItem(i.VariationId, i.Quantity))
            .ToList();

        var result = await inventoryClient.DeductStockAsync(orderId, deductionItems, reason: $"Order {orderId}", ct);

        if (!result.Success)
        {
            var reason = result.FailureCode ?? "InsufficientStock";
            context.Set(CreateOrderSagaContextKeys.FailureReason, reason);

            logger.Warning(
                "DeductInventory failed for order {OrderId}: {FailureCode} ({InsufficientCount} item(s) insufficient)",
                orderId, reason, result.InsufficientItems.Count);

            throw new InventoryDeductionFailedException(reason);
        }

        logger.Information("Inventory deducted for order {OrderId}", orderId);
    }

    /// <summary>Runs only if a later step (ConfirmOrder) fails after this one already succeeded - reverses the deduction so stock isn't lost for an order that never got confirmed.</summary>
    public async Task CompensateAsync(ISagaContext context, CancellationToken ct = default)
    {
        var orderId = context.Get<Guid>(CreateOrderSagaContextKeys.OrderId);

        logger.Warning("Compensating DeductInventory for order {OrderId}: restocking", orderId);
        await inventoryClient.RestockAsync(orderId, reason: "CreateOrderSaga compensation", ct);
    }
}
