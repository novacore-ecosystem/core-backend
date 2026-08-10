using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using NovaCore.Inventory.Persistence.Contexts.InventoryTransactions.Repositories;

namespace NovaCore.Inventory.Persistence.Contexts.InventoryTransactions.Read;

public sealed class InventoryTransactionReadService(IInventoryTransactionRepository repo) : IInventoryTransactionReadService
{
    public async Task<IReadOnlyList<InventoryTransaction>> GetHistoryAsync(Guid inventoryId, CancellationToken ct = default)
    {
        return await repo.GetHistoryAsync(inventoryId, ct);
    }

    public async Task<PaginatedResult<InventoryTransaction>> SearchAsync(CriteriaRequest request, CancellationToken ct = default)
    {
        return await repo.SearchAsync(request, ct);
    }
}
