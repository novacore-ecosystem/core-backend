using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.Persistence.Ef.Criteria;
using NovaCore.Inventory.Application.Features.Warehouses.Search;
using NovaCore.Inventory.Persistence.Engine;

namespace NovaCore.Inventory.Persistence.Contexts.Warehouses.Repositories;

public sealed class WarehouseRepo(InventoryDbContext dbContext)
    : InventoryBaseRepository<Warehouse, Guid>(dbContext), IWarehouseRepository
{
    public async Task<Warehouse?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _dbContext.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Code == code, ct);
    }

    public async Task<PaginatedResult<Warehouse>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default)
    {
        return await _dbContext.Warehouses
            .AsNoTracking()
            .ApplyCriteria(WarehouseCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
    }
}
