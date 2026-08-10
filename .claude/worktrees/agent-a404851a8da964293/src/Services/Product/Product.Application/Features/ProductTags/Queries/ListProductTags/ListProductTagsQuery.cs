namespace NovaCore.Product.Application.Features.ProductTags.Queries.ListProductTags;

public sealed record ListProductTagsQuery : IQuery<ListProductTagsResponse>;

public sealed record ProductTagItemResponse(Guid Id, string Code, string Name);

public sealed record ListProductTagsResponse(IReadOnlyCollection<ProductTagItemResponse> Tags);
