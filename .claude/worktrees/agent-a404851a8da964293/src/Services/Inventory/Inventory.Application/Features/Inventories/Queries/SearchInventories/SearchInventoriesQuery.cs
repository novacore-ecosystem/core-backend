using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;

namespace NovaCore.Inventory.Application.Features.Inventories.Queries.SearchInventories;

public sealed record SearchInventoriesQuery(CriteriaRequest Criteria) : IQuery<PaginatedResult<SearchInventoriesItemResponse>>;

public sealed record SearchInventoriesItemResponse(
    Guid Id,
    Guid ProductId,
    Guid VariantId,
    Guid WarehouseId,
    int Quantity,
    DateTime CreatedAt,
    DateTime UpdatedAt);
