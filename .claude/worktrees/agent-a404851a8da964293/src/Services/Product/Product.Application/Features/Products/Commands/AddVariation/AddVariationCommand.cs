using NovaCore.Product.Application.Features.Products.DTOs;

namespace NovaCore.Product.Application.Features.Products.Commands.AddVariation;

public sealed record AddVariationCommand(
    Guid ProductId,
    VariantInputDto VariationInput) : ICommand<AddVariationResponse>;

public sealed record AddVariationResponse(VariantResponse Variation);
