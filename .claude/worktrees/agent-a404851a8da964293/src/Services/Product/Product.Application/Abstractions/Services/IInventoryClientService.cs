namespace NovaCore.Product.Application.Abstractions.Services;

/// <summary>
/// Product-side port onto Inventory Service's gRPC surface. Mirrors NovaCore.Order.Application's
/// IInventoryClientService (same batch RPC), used only to merge stock availability into
/// Search Products - Product never deducts/restocks (see docs/services/inventory-service.md).
/// </summary>
public interface IInventoryClientService
{
    /// <summary>One gRPC round trip for every requested variation on a search results page - avoids an N+1. Variation ids with no inventory rows come back as 0, not missing.</summary>
    Task<IReadOnlyDictionary<Guid, int>> GetAvailableStockBatchAsync(IReadOnlyCollection<Guid> productVariationIds, CancellationToken ct = default);
}
