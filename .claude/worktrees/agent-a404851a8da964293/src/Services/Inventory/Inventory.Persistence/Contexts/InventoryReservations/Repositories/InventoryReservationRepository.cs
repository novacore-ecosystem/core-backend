using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.Persistence.Ef.Criteria;
using NovaCore.Inventory.Application.Features.Inventories.Search;
using NovaCore.Inventory.Application.Features.InventoryReservations.Search;
using NovaCore.Inventory.Persistence.Engine;

namespace NovaCore.Inventory.Persistence.Contexts.InventoryReservations.Repositories;

public sealed class InventoryReservationRepository(InventoryDbContext dbContext)
    : InventoryBaseRepository<InventoryReservation, Guid>(dbContext), IInventoryReservationRepository
{
    public async Task<InventoryReservation?> GetByNumberAsync(
        string number,
        CancellationToken ct = default)
    {
        return await _dbContext.InventoryReservations
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Number == number, ct);
    }

    public async Task<IReadOnlyList<InventoryReservation>> GetByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default)
    {
        return await _dbContext.InventoryReservations
            .AsNoTracking()
            .Where(r => r.InventoryId == inventoryId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<InventoryReservation>> GetActiveByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default)
    {
        return await _dbContext.InventoryReservations
            .AsNoTracking()
            .Where(r => r.InventoryId == inventoryId &&
                        (r.Status == InventoryReservationStatus.Pending ||
                         r.Status == InventoryReservationStatus.Reserved ||
                         r.Status == InventoryReservationStatus.Committed))
            .ToListAsync(ct);
    }

    public async Task<PaginatedResult<InventoryReservation>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default)
    {
        return await _dbContext.InventoryReservations
            .AsNoTracking()
            .ApplyCriteria(InventoryReservationCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
    }
}
