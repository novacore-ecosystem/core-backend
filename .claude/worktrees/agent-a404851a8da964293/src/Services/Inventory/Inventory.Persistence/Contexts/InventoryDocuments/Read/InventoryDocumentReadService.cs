using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.Inventory.Application.Abstractions.Persistence.InventoryDocuments;
using NovaCore.Inventory.Persistence.Contexts.InventoryDocuments.Repositories;

namespace NovaCore.Inventory.Persistence.Contexts.InventoryDocuments.Read;

public sealed class InventoryDocumentReadService(IInventoryDocumentRepository repo) : IInventoryDocumentReadService
{
    public async Task<InventoryDocument?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await repo.GetByIdAsync(id, ct);
    }

    public async Task<PaginatedResult<InventoryDocument>> SearchAsync(CriteriaRequest request, CancellationToken ct = default)
    {
        return await repo.SearchAsync(request, ct);
    }

    public async Task<InventoryDocument?> GetByNumberAsync(string number, CancellationToken ct = default)
    {
        return await repo.GetByNumberAsync(number, ct);
    }

    public async Task<IReadOnlyList<InventoryDocument>> GetBySourceWarehouseIdAsync(Guid sourceWarehouseId, CancellationToken ct = default)
    {
        return await repo.GetBySourceWarehouseIdAsync(sourceWarehouseId, ct);
    }

    public async Task<IReadOnlyList<InventoryDocument>> GetByDestinationWarehouseIdAsync(Guid destinationWarehouseId, CancellationToken ct = default)
    {
        return await repo.GetByDestinationWarehouseIdAsync(destinationWarehouseId, ct);
    }
}
