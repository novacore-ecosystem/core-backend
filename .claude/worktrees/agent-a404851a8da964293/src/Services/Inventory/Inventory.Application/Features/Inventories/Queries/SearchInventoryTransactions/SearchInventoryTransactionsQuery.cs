using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;

namespace NovaCore.Inventory.Application.Features.Inventories.Queries.SearchInventoryTransactions;

public sealed record SearchInventoryTransactionsQuery(CriteriaRequest Criteria) : IQuery<PaginatedResult<SearchInventoryTransactionsItemResponse>>;

public sealed record SearchInventoryTransactionsItemResponse(
    Guid Id,
    Guid InventoryId,
    Guid ProductId,
    Guid VariantId,
    Guid WarehouseId,
    InventoryTransactionType Type,
    int Quantity,
    int QuantityAfter,
    string Reason,
    DateTime CreatedAt);
