namespace NovaCore.Product.Application.Features.Products.Commands.UpdateProduct;

/// <summary>Product-level info only - never touches Variant, see Variant-specific commands for that.</summary>
public sealed record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string Description,
    string Slug) : ICommand<UpdateProductResponse>;

public sealed record UpdateProductResponse;
