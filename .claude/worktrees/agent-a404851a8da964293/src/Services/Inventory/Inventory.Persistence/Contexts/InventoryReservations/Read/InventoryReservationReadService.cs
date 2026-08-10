using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.Inventory.Application.Abstractions.Persistence.InventoryReservations;
using NovaCore.Inventory.Persistence.Contexts.InventoryReservations.Repositories;

namespace NovaCore.Inventory.Persistence.Contexts.InventoryReservations.Read;

public sealed class InventoryReservationReadService(IInventoryReservationRepository repo) : IInventoryReservationReadService
{
    public async Task<InventoryReservation?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await repo.GetByIdAsync(id, ct);
    }

    public async Task<PaginatedResult<InventoryReservation>> SearchAsync(CriteriaRequest request, CancellationToken ct = default)
    {
        return await repo.SearchAsync(request, ct);
    }

    public async Task<InventoryReservation?> GetByNumberAsync(string number, CancellationToken ct = default)
    {
        return await repo.GetByNumberAsync(number, ct);
    }

    public async Task<IReadOnlyList<InventoryReservation>> GetByInventoryIdAsync(Guid inventoryId, CancellationToken ct = default)
    {
        return await repo.GetByInventoryIdAsync(inventoryId, ct);
    }

    public async Task<IReadOnlyList<InventoryReservation>> GetActiveByInventoryIdAsync(Guid inventoryId, CancellationToken ct = default)
    {
        return await repo.GetActiveByInventoryIdAsync(inventoryId, ct);
    }
}
