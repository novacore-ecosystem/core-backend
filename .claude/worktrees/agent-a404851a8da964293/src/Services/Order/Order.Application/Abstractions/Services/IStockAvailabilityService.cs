namespace NovaCore.Order.Application.Abstractions.Services;

/// <summary>
/// Single source of truth for "is there enough stock for these variations" - shared by Create
/// Order, Add Cart, and Get Cart so each stops reimplementing the Inventory batch-check
/// independently (see docs/tasks/2026-07-27/Task17_no-shared-stock-validation-service.md).
/// </summary>
public interface IStockAvailabilityService
{
    /// <summary>One batched Inventory gRPC call for every requested (variationId, quantity) pair. Never throws for insufficiency - callers decide how to react (reject outright, mark specific items, etc.).</summary>
    Task<IReadOnlyDictionary<Guid, StockAvailability>> CheckAsync(
        IReadOnlyCollection<StockRequest> requests,
        CancellationToken ct = default);
}

public sealed record StockRequest(Guid VariationId, int RequestedQuantity);

public sealed record StockAvailability(Guid VariationId, int RequestedQuantity, int AvailableQuantity)
{
    public bool IsSufficient => AvailableQuantity >= RequestedQuantity;
}
