namespace NovaCore.Inventory.Application.Abstractions.Persistence.Inventories;

public interface IInventoryReadService
{
    Task<InventoryStock?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Loads multiple inventories in a single database query, reducing N+1 problems.</summary>
    Task<IReadOnlyList<InventoryStock>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);

    Task<PaginatedResult<InventoryStock>> SearchAsync(CriteriaRequest request, CancellationToken ct = default);

    /// <summary>Stock is keyed at (VariantId, WarehouseId) - each product variation per warehouse has one InventoryStock aggregate.</summary>
    Task<InventoryStock?> GetByVariationAndWarehouseAsync(
        Guid productVariationId,
        Guid warehouseId,
        CancellationToken ct = default);

    /// <summary>Rollup across every variation and warehouse for the whole product.</summary>
    Task<int> GetTotalStockByProductIdAsync(Guid productId, CancellationToken ct = default);

    /// <summary>Rollup across every warehouse for a single variation.</summary>
    Task<int> GetTotalStockByVariationIdAsync(Guid productVariationId, CancellationToken ct = default);

    /// <summary>Batched rollup across every warehouse for each requested variation, in one query. Variation ids with no inventory rows are absent from the result (callers should default to 0).</summary>
    Task<IReadOnlyDictionary<Guid, int>> GetTotalStockByVariationIdsAsync(IReadOnlyCollection<Guid> productVariationIds, CancellationToken ct = default);
}
