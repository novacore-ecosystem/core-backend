namespace NovaCore.Product.Application.Features.ProductTags.Commands.CreateProductTag;

public sealed record CreateProductTagCommand(string Code, string Name) : ICommand<CreateProductTagResponse>;

public sealed record CreateProductTagResponse(Guid ProductTagId);
