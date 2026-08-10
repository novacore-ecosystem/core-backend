using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Contract.Events.Product;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Product.Application.Abstractions.Persistence.ProductCategories;
using NovaCore.Product.Application.Abstractions.Persistence.Products;
using NovaCore.Product.Application.Abstractions.Persistence.ProductTags;
using NovaCore.Product.Application.Features.Products.DTOs;

namespace NovaCore.Product.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductHandler(
    IProductReadService productReadService,
    IProductWriteService productWriteService,
    IProductCategoryReadService categoryReadService,
    IProductTagReadService tagReadService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<CreateProductCommand, CreateProductResponse>
{
    public async Task<CreateProductResponse> Handle(CreateProductCommand request, CancellationToken ct = default)
    {
        // Validate request and referenced resources.
        await ValidateRequestAsync(request, ct);

        var categoryIds = request.CategoryIds ?? [];
        await ValidateCategoriesAsync(categoryIds, ct);

        var tagIds = request.TagIds ?? [];
        await ValidateTagsAsync(tagIds, ct);

        // Persist data and enqueue integration events.
        var correlationId = currentUser.GetCorrelationId();
        var product = await SaveProductAsync(request, categoryIds, tagIds, correlationId, ct);

        // Build response.
        return new CreateProductResponse(product.Id, product.DefaultVariation.Id);
    }

    #region Validation
    private async Task ValidateRequestAsync(CreateProductCommand request, CancellationToken ct)
    {
        if (request.Variations.Count == 0)
            throw new BadRequestException("A product must be created with at least one variation.");

        if (await productReadService.CodeExistsAsync(request.Code, ct))
            throw new ConflictException($"Product with code ({request.Code}) already exists");

        if (await productReadService.SlugExistsAsync(request.Slug, ct: ct))
            throw new ConflictException($"Product with slug ({request.Slug}) already exists");

        foreach (var variation in request.Variations)
        {
            if (await productReadService.SkuExistsAsync(Sku.Create(variation.Sku), ct: ct))
                throw new ConflictException($"Variation with SKU ({variation.Sku}) already exists");
        }
    }

    private async Task ValidateCategoriesAsync(IReadOnlyCollection<Guid> categoryIds, CancellationToken ct)
    {
        var existingCategoryIds = await categoryReadService.GetExistingIdsAsync(categoryIds, ct);
        var nonExistIds = categoryIds.Except(existingCategoryIds)
            .Select(t => t.ToString())
            .ToArray();
        if (nonExistIds.Length > 0)
            throw new NotFoundException(nameof(ProductCategories), nonExistIds.JoinToString(","));
    }

    private async Task ValidateTagsAsync(IReadOnlyCollection<Guid> tagIds, CancellationToken ct)
    {
        var existingTagIds = await tagReadService.GetExistingTagIdsAsync(tagIds, ct);
        var nonExistIds = tagIds.Except(existingTagIds)
            .Select(t => t.ToString())
            .ToArray();
        if (nonExistIds.Length > 0)
            throw new NotFoundException(nameof(ProductTag), nonExistIds.JoinToString(","));
    }
    #endregion

    #region Persistence
    private async Task<ProductEntity> SaveProductAsync(
        CreateProductCommand request,
        IReadOnlyCollection<Guid> categoryIds,
        IReadOnlyCollection<Guid> tagIds,
        string correlationId,
        CancellationToken ct)
    {
        ProductEntity product = null!;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            product = await productWriteService.CreateAsync(
                new CreateProductRequest(
                    request.Code,
                    request.Name,
                    request.Description,
                    request.Slug,
                    request.Variations,
                    categoryIds,
                    tagIds),
                ct);
            await PublishIntegrationEventsAsync(product, correlationId, ct);
        }, ct: ct);

        return product;
    }

    private async Task PublishIntegrationEventsAsync(
        ProductEntity product,
        string correlationId,
        CancellationToken ct)
    {
        await outboxStore.EnqueueAsync(
            new ProductCreatedIntegrationEvent(
                product.Id,
                product.Code.Value,
                product.Name,
                product.Slug.Value,
                correlationId),
            ct);

        foreach (var variation in product.Variations)
        {
            await outboxStore.EnqueueAsync(
                new VariantCreatedIntegrationEvent(
                    product.Id,
                    variation.Id,
                    variation.Sku.Value,
                    product.Name,
                    variation.Name,
                    variation.Price.Value,
                    variation.Status.ToString(),
                    correlationId),
                ct);
        }
    }
    #endregion
}
