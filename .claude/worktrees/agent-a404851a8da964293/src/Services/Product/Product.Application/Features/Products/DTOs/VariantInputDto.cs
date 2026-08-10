namespace NovaCore.Product.Application.Features.Products.DTOs;

public sealed record VariantInputDto(
    string Sku,
    string Name,
    decimal Price,
    bool IsDefault = false,
    string? Barcode = null,
    decimal? Weight = null,
    string? WeightUnit = null,
    decimal? DimensionsLength = null,
    decimal? DimensionsWidth = null,
    decimal? DimensionsHeight = null,
    IReadOnlyCollection<string>? Images = null
);
