using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Inventory.Persistence.Contexts.InventoryTransactions.Repositories;

public interface IInventoryTransactionRepository : IRepository<InventoryTransaction, Guid>
{
    Task<IReadOnlyList<InventoryTransaction>> GetHistoryAsync(
        Guid inventoryId,
        CancellationToken ct = default);

    Task<PaginatedResult<InventoryTransaction>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);
}
