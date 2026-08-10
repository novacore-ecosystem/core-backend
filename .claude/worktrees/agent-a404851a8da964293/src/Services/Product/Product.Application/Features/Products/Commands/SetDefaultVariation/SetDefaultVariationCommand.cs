namespace NovaCore.Product.Application.Features.Products.Commands.SetDefaultVariation;

public sealed record SetDefaultVariationCommand(Guid ProductId, Guid VariationId) : ICommand<SetDefaultVariationResponse>;

public sealed record SetDefaultVariationResponse;
