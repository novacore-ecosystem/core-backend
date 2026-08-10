using NovaCore.BuildingBlock.Application.Abstractions.Common;

using NovaCore.Inventory.Application.Abstractions.Persistence.InventoryTransactions;

using Mapster;

namespace NovaCore.Inventory.Application.Features.Inventories.Queries.SearchInventoryTransactions;

public sealed class SearchInventoryTransactionsHandler(IInventoryTransactionReadService transactionReadService)
    : IQueryHandler<SearchInventoryTransactionsQuery, PaginatedResult<SearchInventoryTransactionsItemResponse>>
{
    public async Task<PaginatedResult<SearchInventoryTransactionsItemResponse>> Handle(SearchInventoryTransactionsQuery request, CancellationToken ct = default)
    {
        var result = await transactionReadService.SearchAsync(request.Criteria, ct);

        var items = result.Items.Select(t => t.Adapt<SearchInventoryTransactionsItemResponse>()).ToList();

        return PaginatedResult<SearchInventoryTransactionsItemResponse>.Create(items, result.PageNumber, result.PageSize, result.TotalCount);
    }
}
