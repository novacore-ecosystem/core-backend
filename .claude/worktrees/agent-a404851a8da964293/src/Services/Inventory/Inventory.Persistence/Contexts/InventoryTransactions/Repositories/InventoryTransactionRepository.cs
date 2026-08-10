using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.Persistence.Ef.Criteria;
using NovaCore.Inventory.Application.Features.Inventories.Search;
using NovaCore.Inventory.Persistence.Engine;

namespace NovaCore.Inventory.Persistence.Contexts.InventoryTransactions.Repositories;

public sealed class InventoryTransactionRepository(InventoryDbContext dbContext)
    : InventoryBaseRepository<InventoryTransaction, Guid>(dbContext), IInventoryTransactionRepository
{
    public async Task<IReadOnlyList<InventoryTransaction>> GetHistoryAsync(
        Guid inventoryId,
        CancellationToken ct = default)
    {
        return await _dbContext.InventoryTransactions
            .AsNoTracking()
            .Where(t => t.InventoryId == inventoryId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<PaginatedResult<InventoryTransaction>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default)
    {
        return await _dbContext.InventoryTransactions
            .AsNoTracking()
            .ApplyCriteria(InventoryTransactionCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
    }
}
