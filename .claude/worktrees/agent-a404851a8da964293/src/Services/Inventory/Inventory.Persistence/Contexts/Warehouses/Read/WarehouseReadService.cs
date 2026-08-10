using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.Inventory.Application.Abstractions.Persistence.Warehouses;
using NovaCore.Inventory.Persistence.Contexts.Warehouses.Repositories;

namespace NovaCore.Inventory.Persistence.Contexts.Warehouses.Read;

public sealed class WarehouseReadService(IWarehouseRepository repo) : IWarehouseReadService
{
    public async Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await repo.GetByIdAsync(id, ct);
    }

    public async Task<Warehouse?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await repo.GetByCodeAsync(code, ct);
    }

    public async Task<PaginatedResult<Warehouse>> SearchAsync(CriteriaRequest request, CancellationToken ct = default)
    {
        return await repo.SearchAsync(request, ct);
    }
}
