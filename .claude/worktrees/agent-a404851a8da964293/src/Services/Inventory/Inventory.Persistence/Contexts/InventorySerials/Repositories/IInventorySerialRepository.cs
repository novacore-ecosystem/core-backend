using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Inventory.Persistence.Contexts.InventorySerials.Repositories;

public interface IInventorySerialRepository : IRepository<InventorySerial, Guid>
{
    Task<InventorySerial?> GetBySerialNumberAsync(
        string serialNumber,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventorySerial>> GetByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventorySerial>> GetAvailableByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default);

    Task<PaginatedResult<InventorySerial>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);
}
