namespace NovaCore.Product.Application.Features.ProductCategories.Queries.ListProductCategories;

/// <summary>
/// Returns the full flat category set (not paginated - category counts are small reference
/// data) so a client can assemble the tree itself from Id/ParentCategoryId pairs.
/// </summary>
public sealed record ListProductCategoriesQuery : IQuery<ListProductCategoriesResponse>;

public sealed record ProductCategoryItemResponse(
    Guid Id,
    string Code,
    string Name,
    ProductCategoryStatus Status,
    Guid? ParentCategoryId);

public sealed record ListProductCategoriesResponse(IReadOnlyCollection<ProductCategoryItemResponse> Categories);
