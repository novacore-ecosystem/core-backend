namespace NovaCore.Order.Application.Abstractions.Services;

/// <summary>
/// Order-side port onto Inventory Service's gRPC surface. CreateOrderHandler only calls
/// GetAvailableStockBatchAsync (pre-check, never reserves - see docs/services/order-service.md);
/// DeductStockAsync/RestockAsync are called by CreateOrderSaga's DeductInventoryStep
/// (NovaCore.Order.Infrastructure) for the actual reservation and its compensation.
/// </summary>
public interface IInventoryClientService
{
    /// <summary>One gRPC round trip for every requested variation - avoids an N+1 when validating an order with many items. Variation ids with no inventory rows come back as 0, not missing.</summary>
    Task<IReadOnlyDictionary<Guid, int>> GetAvailableStockBatchAsync(IReadOnlyCollection<Guid> productVariationIds, CancellationToken ct = default);

    Task<InventoryDeductionResult> DeductStockAsync(
        Guid deductionId,
        IReadOnlyCollection<InventoryDeductionItem> items,
        string? reason,
        CancellationToken ct = default);

    Task RestockAsync(Guid deductionId, string? reason, CancellationToken ct = default);
}

public sealed record InventoryDeductionItem(Guid VariantId, int Quantity);

public sealed record InventoryDeductionInsufficientItem(Guid VariantId, int RequestedQuantity, int AvailableQuantity);

public sealed record InventoryDeductionResult(
    bool Success,
    string? FailureCode,
    IReadOnlyCollection<InventoryDeductionInsufficientItem> InsufficientItems);
