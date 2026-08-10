using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Inventory.Application.Abstractions.Persistence.Warehouses;

using Mapster;

namespace NovaCore.Inventory.Application.Features.Warehouses.Queries.GetWarehouse;

public sealed class GetWarehouseHandler(IWarehouseReadService warehouseReadService)
    : IQueryHandler<GetWarehouseQuery, GetWarehouseResponse>
{
    public async Task<GetWarehouseResponse> Handle(GetWarehouseQuery request, CancellationToken ct = default)
    {
        var warehouse = await warehouseReadService.GetByIdAsync(request.WarehouseId, ct)
            ?? throw new NotFoundException("Warehouse", request.WarehouseId);

        return warehouse.Adapt<GetWarehouseResponse>();
    }
}
