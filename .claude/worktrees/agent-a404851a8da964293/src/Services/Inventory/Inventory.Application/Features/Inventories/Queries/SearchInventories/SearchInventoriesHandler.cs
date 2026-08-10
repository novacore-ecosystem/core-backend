using NovaCore.BuildingBlock.Application.Abstractions.Common;

using NovaCore.Inventory.Application.Abstractions.Persistence.Inventories;

using Mapster;

namespace NovaCore.Inventory.Application.Features.Inventories.Queries.SearchInventories;

public sealed class SearchInventoriesHandler(IInventoryReadService inventoryReadService)
    : IQueryHandler<SearchInventoriesQuery, PaginatedResult<SearchInventoriesItemResponse>>
{
    public async Task<PaginatedResult<SearchInventoriesItemResponse>> Handle(SearchInventoriesQuery request, CancellationToken ct = default)
    {
        var result = await inventoryReadService.SearchAsync(request.Criteria, ct);

        var items = result.Items.Select(i => i.Adapt<SearchInventoriesItemResponse>()).ToList();

        return PaginatedResult<SearchInventoriesItemResponse>.Create(items, result.PageNumber, result.PageSize, result.TotalCount);
    }
}
