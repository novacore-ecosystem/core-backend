namespace NovaCore.Product.Application.Features.Products.Commands.RemoveProductTag;

public sealed record RemoveProductTagCommand(Guid ProductId, Guid TagId) : ICommand<RemoveProductTagResponse>;

public sealed record RemoveProductTagResponse;
