namespace NovaCore.Product.Application.Features.Products.Commands.DeleteVariation;

public sealed record DeleteVariationCommand(Guid ProductId, Guid VariationId) : ICommand<DeleteVariationResponse>;

public sealed record DeleteVariationResponse;
