using NovaCore.BuildingBlock.Application.Abstractions.Common;

using NovaCore.Inventory.Application.Abstractions.Persistence.Warehouses;

using Mapster;

namespace NovaCore.Inventory.Application.Features.Warehouses.Queries.SearchWarehouses;

public sealed class SearchWarehousesHandler(IWarehouseReadService warehouseReadService)
    : IQueryHandler<SearchWarehousesQuery, PaginatedResult<SearchWarehousesItemResponse>>
{
    public async Task<PaginatedResult<SearchWarehousesItemResponse>> Handle(SearchWarehousesQuery request, CancellationToken ct = default)
    {
        var result = await warehouseReadService.SearchAsync(request.Criteria, ct);

        var items = result.Items.Select(w => w.Adapt<SearchWarehousesItemResponse>()).ToList();

        return PaginatedResult<SearchWarehousesItemResponse>.Create(items, result.PageNumber, result.PageSize, result.TotalCount);
    }
}
