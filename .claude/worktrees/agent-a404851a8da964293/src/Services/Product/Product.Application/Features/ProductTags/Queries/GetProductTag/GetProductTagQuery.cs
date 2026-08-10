namespace NovaCore.Product.Application.Features.ProductTags.Queries.GetProductTag;

public sealed record GetProductTagQuery(Guid ProductTagId) : IQuery<GetProductTagResponse>;

public sealed record GetProductTagResponse(
    Guid Id,
    string Code,
    string Name,
    DateTime CreatedAt,
    DateTime UpdatedAt);
