using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Inventory.Persistence.Contexts.InventoryCounts.Repositories;

public interface IInventoryCountRepository : IRepository<InventoryCount, Guid>
{
    Task<InventoryCount?> GetByNumberAsync(
        string number,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventoryCount>> GetByWarehouseIdAsync(
        Guid warehouseId,
        CancellationToken ct = default);

    Task<PaginatedResult<InventoryCount>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);
}
