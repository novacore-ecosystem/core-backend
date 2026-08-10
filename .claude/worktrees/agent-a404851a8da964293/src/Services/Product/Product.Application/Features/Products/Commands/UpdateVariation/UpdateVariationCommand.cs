namespace NovaCore.Product.Application.Features.Products.Commands.UpdateVariation;

/// <summary>
/// Covers every Variant attribute except DisplayOrder and IsDefault, which have their
/// own dedicated commands (ReorderVariations / SetDefaultVariation) since they carry
/// cross-variation invariants the aggregate root must mediate.
/// </summary>
public sealed record UpdateVariationCommand(
    Guid ProductId,
    Guid VariationId,
    string Sku,
    string Name,
    decimal Price,
    VariantStatus Status,
    string? Barcode = null,
    decimal? Weight = null,
    WeightUnit? WeightUnit = null,
    decimal? DimensionsLength = null,
    decimal? DimensionsWidth = null,
    decimal? DimensionsHeight = null,
    IReadOnlyCollection<string>? Images = null) : ICommand<UpdateVariationResponse>;

public sealed record UpdateVariationResponse;
