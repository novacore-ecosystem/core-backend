namespace NovaCore.Product.Application.Abstractions.Search;

/// <summary>
/// Query params for Product Search. Extending this later with e.g. PriceMin/PriceMax/Brand is
/// just new optional fields here - no redesign of the repository/query pipeline.
/// </summary>
public sealed record ProductSearchCriteria(
    string? Keyword,
    Guid? CategoryId,
    Guid? TagId,
    string? Status,
    string? SortBy,
    bool SortDescending,
    int Page,
    int PageSize);
