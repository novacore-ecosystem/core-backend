using NovaCore.Product.Application.Features.Products.DTOs;

namespace NovaCore.Product.Application.Abstractions.Persistence.Products;

public interface IProductWriteService
{
    /// <summary>Returns the created ProductEntity - CreateProductHandler needs it whole (Id, DefaultVariation, every variation) to build ProductCreatedIntegrationEvent/VariantCreatedIntegrationEvent per variation.</summary>
    Task<ProductEntity> CreateAsync(CreateProductRequest request, CancellationToken ct = default);

    Task UpdateDetailsAsync(
        Guid id,
        string name,
        string description,
        Slug slug,
        CancellationToken ct = default);

    Task<ProductVariant> AddVariationAsync(
        Guid productId,
        VariantInputDto variation,
        CancellationToken ct = default);

    Task<ProductVariant[]> AddVariationsAsync(
        Guid productId,
        IEnumerable<VariantInputDto> variations,
        CancellationToken ct = default);

    Task<ProductVariant> UpdateVariationInformationAsync(
        Guid productId,
        Guid variationId,
        Sku sku,
        string name,
        Money price,
        Barcode? barcode,
        Weight? weight,
        Dimensions? dimensions,
        IReadOnlyCollection<string>? images,
        VariantStatus? status = null,
        CancellationToken ct = default);

    Task DeleteVariationAsync(Guid variationId, CancellationToken ct = default);

    Task ReorderVariationsAsync(Guid productId, IReadOnlyList<Guid> orderedVariationIds, CancellationToken ct = default);

    Task SetDefaultVariationAsync(Guid productId, Guid variationId, CancellationToken ct = default);

    Task AssignCategoryAsync(Guid productId, Guid categoryId, CancellationToken ct = default);

    Task AssignTagAsync(Guid productId, Guid tagId, CancellationToken ct = default);

    Task RemoveCategoryAsync(Guid productId, Guid categoryId, CancellationToken ct = default);

    Task RemoveTagAsync(Guid productId, Guid tagId, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
