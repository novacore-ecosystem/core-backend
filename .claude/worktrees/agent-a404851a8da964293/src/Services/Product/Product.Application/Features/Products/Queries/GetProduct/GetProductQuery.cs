namespace NovaCore.Product.Application.Features.Products.Queries.GetProduct;

public sealed record GetProductQuery(Guid ProductId) : IQuery<GetProductResponse>;

public sealed record GetProductResponse(
    Guid Id,
    string Code,
    string Name,
    string Description,
    string Slug,
    IReadOnlyCollection<Guid> CategoryIds,
    IReadOnlyCollection<Guid> TagIds,
    IReadOnlyCollection<VariantResponse> Variations,
    DateTime CreatedAt,
    DateTime UpdatedAt);
