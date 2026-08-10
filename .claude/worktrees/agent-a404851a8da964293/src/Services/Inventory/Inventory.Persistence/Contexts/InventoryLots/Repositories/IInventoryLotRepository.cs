using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Inventory.Persistence.Contexts.InventoryLots.Repositories;

public interface IInventoryLotRepository : IRepository<InventoryLot, Guid>
{
    Task<IReadOnlyList<InventoryLot>> GetByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default);

    Task<PaginatedResult<InventoryLot>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);
}
