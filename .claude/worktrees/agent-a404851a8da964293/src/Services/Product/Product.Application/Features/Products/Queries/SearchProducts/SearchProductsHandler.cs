using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Application.Abstractions.Services;

using NovaCore.Product.Application.Abstractions.Search;
using NovaCore.Product.Application.Abstractions.Services;

namespace NovaCore.Product.Application.Features.Products.Queries.SearchProducts;

public sealed class SearchProductsHandler(
    IProductSearchRepository searchRepo,
    IInventoryClientService inventoryClient,
    IAppLogger<SearchProductsHandler> logger)
    : IQueryHandler<SearchProductsQuery, PaginatedResult<SearchProductsItemResponse>>
{
    public async Task<PaginatedResult<SearchProductsItemResponse>> Handle(
        SearchProductsQuery request, CancellationToken ct = default)
    {
        var criteria = new ProductSearchCriteria(
            request.Search, request.CategoryId, request.TagId, request.Status,
            request.SortBy, request.SortDescending, request.Page, request.PageSize);

        var (items, totalCount) = await searchRepo.SearchAsync(criteria, ct);

        var variationIds = items
            .SelectMany(d => d.VariationIds)
            .Distinct()
            .ToList();

        var stockByVariationId = await GetStockByVariationIdAsync(variationIds, ct);

        var mapped = items
            .Select(d => new SearchProductsItemResponse(
                d.ProductId, d.Code, d.Name, d.Slug, d.Thumbnail, d.DefaultPrice,
                d.DefaultVariationId, d.DefaultVariationSku, d.CategoryIds, d.CategoryNames,
                d.TagIds, d.TagNames, d.Status, d.UpdatedAt,
                IsInStock: d.VariationIds.Count == 0
                    ? null
                    : stockByVariationId is null
                        ? null
                        : d.VariationIds.Any(id => stockByVariationId.GetValueOrDefault(id) > 0)))
            .ToList();

        return PaginatedResult<SearchProductsItemResponse>.Create(mapped, request.Page, request.PageSize, (int)totalCount);
    }

    /// <summary>Fail-open: a transient Inventory outage must not break product search, so a failed lookup yields null (unknown) stock for the whole page rather than an error.</summary>
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
                "Inventory Service unreachable while resolving stock availability for {Count} variations during product search - returning unknown stock for this page. {Exception}",
                variationIds.Count, ex.Message);
            return null;
        }
    }
}
