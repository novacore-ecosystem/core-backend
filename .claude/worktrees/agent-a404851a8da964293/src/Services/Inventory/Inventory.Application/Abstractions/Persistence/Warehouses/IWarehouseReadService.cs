using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;

namespace NovaCore.Inventory.Application.Abstractions.Persistence.Warehouses;

public interface IWarehouseReadService
{
    Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Warehouse?> GetByCodeAsync(string code, CancellationToken ct = default);

    Task<PaginatedResult<Warehouse>> SearchAsync(CriteriaRequest request, CancellationToken ct = default);
}
