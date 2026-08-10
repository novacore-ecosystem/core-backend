using NovaCore.Inventory.Application.Abstractions.Persistence.Inventories;

namespace NovaCore.Inventory.Application.Features.Inventories.Queries.GetProductStock;

public sealed class GetProductStockHandler(IInventoryReadService inventoryReadService)
    : IQueryHandler<GetProductStockQuery, GetProductStockResponse>
{
    public async Task<GetProductStockResponse> Handle(GetProductStockQuery request, CancellationToken ct = default)
    {
        var total = request.VariantId is not null
            ? await inventoryReadService.GetTotalStockByVariationIdAsync(request.VariantId.Value, ct)
            : await inventoryReadService.GetTotalStockByProductIdAsync(request.ProductId, ct);

        return new GetProductStockResponse(request.ProductId, request.VariantId, total);
    }
}
