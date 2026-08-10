using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Inventory.Persistence.Contexts.Inventories.Repositories;

public interface IInventoryRepository : IRepository<InventoryStock, Guid>
{
    Task<PaginatedResult<InventoryStock>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);

    Task DeleteByProductIdAsync(Guid productId, CancellationToken ct = default);

    Task DeleteByVariationIdAsync(Guid productVariationId, CancellationToken ct = default);

    Task<int> GetTotalStockByProductIdAsync(Guid productId, CancellationToken ct = default);

    Task<int> GetTotalStockByVariationIdAsync(Guid variationId, CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, int>> GetTotalStockByVariationIdsAsync(
        IReadOnlyCollection<Guid> variationIds,
        CancellationToken ct = default);
}
