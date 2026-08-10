using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Product.Application.Abstractions.Persistence.Products;
using NovaCore.Product.Application.Abstractions.Services;

namespace NovaCore.Product.Application.Features.Products.Queries.GetProduct;

public sealed class GetProductHandler(
    IProductReadService productReadService,
    IInventoryClientService inventoryClient,
    IAppLogger<GetProductHandler> logger)
    : IQueryHandler<GetProductQuery, GetProductResponse>
{
    public async Task<GetProductResponse> Handle(GetProductQuery request, CancellationToken ct = default)
    {
        var product = await productReadService.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        var variationIds = product.Variations.Select(v => v.Id).ToList();
        var stockByVariationId = await GetStockByVariationIdAsync(variationIds, ct);

        return new GetProductResponse(
            product.Id,
            product.Code.Value,
            product.Name,
            product.Description,
            product.Slug.Value,
            [.. product.CategoryMappings.Select(m => m.CategoryId)],
            [.. product.TagMappings.Select(m => m.TagId)],
            [.. product.Variations.Select(v => VariantResponse.From(
                v, stockByVariationId?.GetValueOrDefault(v.Id)))],
            product.CreatedAt,
            product.UpdatedAt);
    }

    /// <summary>Fail-open, matching SearchProductsHandler's convention: a transient Inventory outage must not break Product Detail, so a failed lookup yields null (unknown) stock for every variation rather than an error.</summary>
    private async Task<IReadOnlyDictionary<Guid, int>?> GetStockByVariationIdAsync(
        IReadOnlyList<Guid> variationIds, CancellationToken ct)
    {
        if (variationIds.Count == 0) return null;

        try
        {
            return await inventoryClient.GetAvailableStockBatchAsync(variationIds, ct);
        }
        catch (Exception ex)
        {
            logger.Warning(
                "Inventory Service unreachable while resolving stock availability for {Count} variations during GetProduct - returning unknown stock. {Exception}",
                variationIds.Count, ex.Message);
            return null;
        }
    }
}
