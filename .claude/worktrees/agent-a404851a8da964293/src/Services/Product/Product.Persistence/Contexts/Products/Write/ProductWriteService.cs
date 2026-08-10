using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Persistence.Repository;

using NovaCore.Product.Application.Abstractions.Persistence.Products;
using NovaCore.Product.Application.Features.Products.DTOs;
using NovaCore.Product.Application.Features.Products.Mapping;
using NovaCore.Product.Persistence.Contexts.Products.Repositories;

namespace NovaCore.Product.Persistence.Contexts.Products.Write;

public sealed class ProductWriteService(
    IRepository<ProductEntity, Guid> repo,
    IProductRepository productRepo,
    IUnitOfWork uow) : IProductWriteService
{
    public async Task<ProductEntity> CreateAsync(
        CreateProductRequest request,
        CancellationToken ct = default)
    {
        var variations = request.Variations.MapInputToEntities(default);
        var product = ProductEntity.Create(
            ProductCode.Create(request.Code),
            request.Name,
            request.Description,
            Slug.Create(request.Slug),
            null,
            variations,
            request.CategoryIds,
            request.TagIds);

        await repo.AddAsync(product, ct);
        await uow.SaveChangesAsync(ct);

        return product;
    }

    public async Task UpdateDetailsAsync(
        Guid id,
        string name,
        string description,
        Slug slug,
        CancellationToken ct = default)
    {
        await repo.UpdateAsync(id, async product =>
        {
            product.UpdateDetails(name, description);
            product.ChangeSlug(slug);
        }, ct);
        await uow.SaveChangesAsync(ct);
    }

    public async Task<ProductVariant> AddVariationAsync(
        Guid productId,
        VariantInputDto variation,
        CancellationToken ct = default)
    {
        var displayOrder = await productRepo.GetNextVariationDisplayOrderAsync(productId, ct);
        var entity = variation.MapInputToEntity(productId, displayOrder);
        await productRepo.AddVariationAsync(entity, ct);
        await uow.SaveChangesAsync(ct);

        return entity;
    }

    public async Task<ProductVariant[]> AddVariationsAsync(
        Guid productId,
        IEnumerable<VariantInputDto> variations,
        CancellationToken ct = default)
    {
        var displayOrder = await productRepo.GetNextVariationDisplayOrderAsync(productId, ct);
        var entities = variations.MapInputToEntities(productId, displayOrder);
        await productRepo.AddVariationRangeAsync(entities, ct);
        await uow.SaveChangesAsync(ct);

        return [.. entities];
    }

    public async Task<ProductVariant> UpdateVariationInformationAsync(
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
        CancellationToken ct = default)
    {
        ProductVariant updated = null!;
        await productRepo.UpdateVariationAsync(
            variationId,
            query => query.Include(q => q.Product),
            async variation =>
            {
                variation.UpdateIdentifiers(sku, barcode);
                variation.UpdateName(name);
                variation.UpdatePricing(price);
                variation.UpdatePhysicalAttributes(weight, dimensions);
                variation.ReplaceImages(images ?? []);

                switch (status)
                {
                    case VariantStatus.Active:
                        variation.Activate();
                        break;
                    case VariantStatus.Inactive:
                        variation.Deactivate();
                        break;
                    case VariantStatus.Discontinued:
                        variation.Discontinue();
                        break;
                }

                updated = variation;
                await uow.SaveChangesAsync(ct);
            },
            ct);

        return updated;
    }

    public async Task DeleteVariationAsync(Guid variationId, CancellationToken ct = default)
    {
        await productRepo.RemoveVariationAsync(variationId, ct);
        await uow.SaveChangesAsync(ct);
    }

    public async Task ReorderVariationsAsync(Guid productId, IReadOnlyList<Guid> orderedVariationIds, CancellationToken ct = default)
    {
        await repo.UpdateAsync(
            productId,
            query => query.Include(q => q.Variations),
            async product =>
            {
                var currentIds = product.Variations.Select(v => v.Id).ToHashSet();
                var requestedIds = orderedVariationIds.ToHashSet();

                if (requestedIds.Count != orderedVariationIds.Count || !currentIds.SetEquals(requestedIds))
                {
                    throw new BadRequestException(
                        "OrderedVariationIds must contain exactly every existing variation id, each exactly once.");
                }

                for (var i = 0; i < orderedVariationIds.Count; i++)
                {
                    var variation = product.Variations.First(v => v.Id == orderedVariationIds[i]);
                    variation.ChangeDisplayOrder(i);
                }

                await uow.SaveChangesAsync(ct);
            }, ct);
    }

    public async Task SetDefaultVariationAsync(Guid productId, Guid variationId, CancellationToken ct = default)
    {
        await repo.UpdateAsync(
            productId,
            query => query.Include(q => q.Variations),
            async product =>
            {
                product.SetDefaultVariation(variationId);

                await uow.SaveChangesAsync(ct);
            }, ct);
    }

    public async Task AssignCategoryAsync(Guid productId, Guid categoryId, CancellationToken ct = default)
    {
        await repo.UpdateAsync(
            productId,
            query => query.Include(q => q.CategoryMappings),
            async product =>
            {
                product.AssignCategory(categoryId);

                await uow.SaveChangesAsync(ct);
            }, ct);
    }

    public async Task AssignTagAsync(Guid productId, Guid tagId, CancellationToken ct = default)
    {
        await repo.UpdateAsync(
            productId,
            query => query.Include(q => q.TagMappings),
            async product =>
            {
                product.AssignTag(tagId);

                await uow.SaveChangesAsync(ct);
            }, ct);
    }

    public async Task RemoveCategoryAsync(Guid productId, Guid categoryId, CancellationToken ct = default)
    {
        await repo.UpdateAsync(
            productId,
            query => query.Include(q => q.CategoryMappings),
            async product =>
            {
                product.RemoveCategory(categoryId);

                await uow.SaveChangesAsync(ct);
            }, ct);
    }

    public async Task RemoveTagAsync(Guid productId, Guid tagId, CancellationToken ct = default)
    {
        await repo.UpdateAsync(
            id: productId,
            includes: query => query.Include(q => q.TagMappings),
            updateAction: async product =>
            {
                product.RemoveTag(tagId);

                await uow.SaveChangesAsync(ct);
            },
            ct: ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await repo.DeleteByIdAsync(id, ct);
        await uow.SaveChangesAsync(ct);
    }
}
