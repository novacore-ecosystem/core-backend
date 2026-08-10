using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.Inventory.Application.Abstractions.Persistence.InventorySerials;
using NovaCore.Inventory.Persistence.Contexts.InventorySerials.Repositories;

namespace NovaCore.Inventory.Persistence.Contexts.InventorySerials.Read;

public sealed class InventorySerialReadService(IInventorySerialRepository repo) : IInventorySerialReadService
{
    public async Task<InventorySerial?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await repo.GetByIdAsync(id, ct);
    }

    public async Task<PaginatedResult<InventorySerial>> SearchAsync(CriteriaRequest request, CancellationToken ct = default)
    {
        return await repo.SearchAsync(request, ct);
    }

    public async Task<InventorySerial?> GetBySerialNumberAsync(string serialNumber, CancellationToken ct = default)
    {
        return await repo.GetBySerialNumberAsync(serialNumber, ct);
    }

    public async Task<IReadOnlyList<InventorySerial>> GetByInventoryIdAsync(Guid inventoryId, CancellationToken ct = default)
    {
        return await repo.GetByInventoryIdAsync(inventoryId, ct);
    }

    public async Task<IReadOnlyList<InventorySerial>> GetAvailableByInventoryIdAsync(Guid inventoryId, CancellationToken ct = default)
    {
        return await repo.GetAvailableByInventoryIdAsync(inventoryId, ct);
    }
}
