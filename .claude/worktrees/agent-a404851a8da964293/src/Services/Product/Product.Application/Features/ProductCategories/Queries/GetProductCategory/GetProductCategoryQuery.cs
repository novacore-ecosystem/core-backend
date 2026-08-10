namespace NovaCore.Product.Application.Features.ProductCategories.Queries.GetProductCategory;

public sealed record GetProductCategoryQuery(Guid ProductCategoryId) : IQuery<GetProductCategoryResponse>;

public sealed record GetProductCategoryResponse(
    Guid Id,
    string Code,
    string Name,
    string Description,
    ProductCategoryStatus Status,
    Guid? ParentCategoryId,
    IReadOnlyCollection<Guid> ChildCategoryIds,
    DateTime CreatedAt,
    DateTime UpdatedAt);
