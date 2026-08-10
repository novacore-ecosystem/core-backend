namespace NovaCore.Product.Application.Features.Products.DTOs;

public sealed record CreateProductRequest(
    string Code,
    string Name,
    string Description,
    string Slug,
    IReadOnlyCollection<VariantInputDto> Variations,
    IReadOnlyCollection<Guid> CategoryIds,
    IReadOnlyCollection<Guid> TagIds);
