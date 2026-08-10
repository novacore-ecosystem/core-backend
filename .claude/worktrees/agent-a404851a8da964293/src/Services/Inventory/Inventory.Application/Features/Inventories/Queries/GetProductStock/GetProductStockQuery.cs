namespace NovaCore.Inventory.Application.Features.Inventories.Queries.GetProductStock;

/// <summary>
/// ProductId alone returns the total stock across every variation and warehouse for that
/// product; ProductId + VariantId narrows the rollup to that single variation. Backs
/// both the REST endpoint and the gRPC stock query Order Service consumes.
/// </summary>
public sealed record GetProductStockQuery(Guid ProductId, Guid? VariantId = null) : IQuery<GetProductStockResponse>;

public sealed record GetProductStockResponse(Guid ProductId, Guid? VariantId, int TotalQuantity);
