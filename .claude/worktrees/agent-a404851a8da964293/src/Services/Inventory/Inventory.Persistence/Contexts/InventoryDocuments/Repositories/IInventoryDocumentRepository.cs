using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Inventory.Persistence.Contexts.InventoryDocuments.Repositories;

public interface IInventoryDocumentRepository : IRepository<InventoryDocument, Guid>
{
    Task<InventoryDocument?> GetByNumberAsync(
        string number,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventoryDocument>> GetBySourceWarehouseIdAsync(
        Guid sourceWarehouseId,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventoryDocument>> GetByDestinationWarehouseIdAsync(
        Guid destinationWarehouseId,
        CancellationToken ct = default);

    Task<PaginatedResult<InventoryDocument>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);
}
