namespace NovaCore.Product.Application.Features.ProductTags.Commands.UpdateProductTag;

public sealed record UpdateProductTagCommand(Guid ProductTagId, string Name) : ICommand<UpdateProductTagResponse>;

public sealed record UpdateProductTagResponse;
