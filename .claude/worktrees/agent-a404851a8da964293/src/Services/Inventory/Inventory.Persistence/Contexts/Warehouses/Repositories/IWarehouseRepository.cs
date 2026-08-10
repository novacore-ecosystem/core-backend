using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Inventory.Persistence.Contexts.Warehouses.Repositories;

public interface IWarehouseRepository : IRepository<Warehouse, Guid>
{
    Task<Warehouse?> GetByCodeAsync(string code, CancellationToken ct = default);

    Task<PaginatedResult<Warehouse>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);
}
