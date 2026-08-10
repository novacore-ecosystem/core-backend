using NovaCore.Inventory.Application.Features.Inventories.DTOs;

namespace NovaCore.Inventory.Application.Abstractions.Persistence.Inventories;

public interface IInventoryWriteService
{
    /// <summary>Self-committing (ExecuteTransactionAsync). Used by OnVariantCreated - the only write in its transaction.</summary>
    Task AddAsync(CreateInventoryRequest request, CancellationToken ct = default);

    /// <summary>Self-committing. Used by OnProductDeleted - the only write in its transaction.</summary>
    Task DeleteByProductIdAsync(Guid productId, CancellationToken ct = default);

    /// <summary>Self-committing. Used by OnVariantDeleted - the only write in its transaction.</summary>
    Task DeleteByVariationIdAsync(Guid productVariationId, CancellationToken ct = default);

    /// <summary>
    /// Non-committing: receives stock and returns the updated entity. Callers (StockIn/RestockStock)
    /// also stage an InventoryTransaction and own the ExecuteTransactionAsync call themselves.
    /// </summary>
    Task<InventoryStock> ReceiveStockAsync(Guid id, int quantity, CancellationToken ct = default);

    /// <summary>
    /// Non-committing: deducts stock (guards against overselling) and returns the updated entity.
    /// Callers (DeductStock/StockOut) also stage an InventoryTransaction and own the
    /// ExecuteTransactionAsync call themselves.
    /// </summary>
    Task<InventoryStock> DeductStockAsync(Guid id, int quantity, CancellationToken ct = default);

    /// <summary>
    /// Non-committing: corrects stock to newQuantity (e.g. after a physical count) and returns the
    /// updated entity plus the delta applied, for transaction logging. Caller (AdjustStock) also
    /// stages an InventoryTransaction and owns the ExecuteTransactionAsync call itself.
    /// </summary>
    Task<InventoryAdjustmentResult> AdjustStockAsync(Guid id, int newQuantity, CancellationToken ct = default);
}
