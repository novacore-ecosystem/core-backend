using NovaCore.BuildingBlock.Contract.Protos.Inventory;

using NovaCore.Product.Application.Abstractions.Services;

namespace NovaCore.Product.Infrastructure.GrpcClients;

/// <summary>Thin adapter over InventoryGrpcService - maps to/from the generated proto types, no business logic here (see docs/services/inventory-service.md).</summary>
public sealed class InventoryClientService(InventoryGrpcService.InventoryGrpcServiceClient client) : IInventoryClientService
{
    public async Task<IReadOnlyDictionary<Guid, int>> GetAvailableStockBatchAsync(IReadOnlyCollection<Guid> productVariationIds, CancellationToken ct = default)
    {
        var request = new GetProductsStockRequest();
        request.VariantIds.AddRange(productVariationIds.Select(id => id.ToString()));

        var response = await client.GetProductsStockAsync(request, cancellationToken: ct);

        return response.Items.ToDictionary(
            i => Guid.Parse(i.VariantId),
            i => i.TotalQuantity);
    }
}
