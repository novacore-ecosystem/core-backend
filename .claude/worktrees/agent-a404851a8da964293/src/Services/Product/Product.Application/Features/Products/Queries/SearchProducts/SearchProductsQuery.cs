using NovaCore.BuildingBlock.Application.Abstractions.Common;

namespace NovaCore.Product.Application.Features.Products.Queries.SearchProducts;

/// <summary>
/// Served entirely from Elasticsearch - never Postgres. See docs/reference/search.md. Only
/// Product Detail (GetProduct) continues reading Postgres.
/// </summary>
public sealed record SearchProductsQuery(
    string? Search,
    Guid? CategoryId,
    Guid? TagId,
    string? Status,
    string? SortBy,
    bool SortDescending = false,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedResult<SearchProductsItemResponse>>;

public sealed record SearchProductsItemResponse(
    Guid Id,
    string Code,
    string Name,
    string Slug,
    string? Thumbnail,
    decimal? DefaultPrice,
    Guid? DefaultVariationId,
    string? DefaultVariationSku,
    IReadOnlyList<Guid> CategoryIds,
    IReadOnlyList<string> CategoryNames,
    IReadOnlyList<Guid> TagIds,
    IReadOnlyList<string> TagNames,
    string Status,
    DateTime UpdatedAt,
    // True when ANY Active variation has stock > 0 (not just the Default variation). Null when
    // the product has no Active variations, or when Inventory Service couldn't be reached for
    // this page (fail-open - a transient Inventory outage must not break product search).
    bool? IsInStock);
