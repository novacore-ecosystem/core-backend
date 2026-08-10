namespace NovaCore.Inventory.Application.Features.Inventories.Queries.GetInventoryHistory;

public sealed record GetInventoryHistoryQuery(Guid InventoryId) : IQuery<GetInventoryHistoryResponse>;

public sealed record InventoryTransactionDto(
    Guid Id,
    InventoryTransactionType Type,
    int Quantity,
    int QuantityAfter,
    string Reason,
    DateTime CreatedAt);

public sealed record GetInventoryHistoryResponse(IReadOnlyList<InventoryTransactionDto> Transactions);
