namespace NovaCore.Product.Application.Features.ProductTags.Commands.DeleteProductTag;

public sealed record DeleteProductTagCommand(Guid ProductTagId) : ICommand<DeleteProductTagResponse>;

public sealed record DeleteProductTagResponse;
