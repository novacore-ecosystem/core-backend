using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Contract.Events.Product;

using NovaCore.Product.Application.Abstractions.Persistence.Products;
using NovaCore.Product.Application.Features.Products.Mapping;

namespace NovaCore.Product.Application.Features.Products.Commands.UpdateVariation;

public sealed class UpdateVariationHandler(
    IProductReadService productReadService,
    IProductWriteService productWriteService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<UpdateVariationCommand, UpdateVariationResponse>
{
    public async Task<UpdateVariationResponse> Handle(UpdateVariationCommand request, CancellationToken ct = default)
    {
        var sku = Sku.Create(request.Sku);
        if (await productReadService.SkuExistsAsync(sku, request.VariationId, ct))
            throw new ConflictException($"Variation with SKU ({request.Sku}) already exists");

        var dimensions = request.DimensionsLength is not null && request.DimensionsWidth is not null && request.DimensionsHeight is not null
            ? Dimensions.Create(request.DimensionsLength.Value, request.DimensionsWidth.Value, request.DimensionsHeight.Value)
            : null;
        var correlationId = currentUser.GetCorrelationId();

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var variation = await productWriteService.UpdateVariationInformationAsync(
                request.ProductId,
                request.VariationId,
                sku,
                request.Name,
                Money.Create(request.Price),
                request.Barcode is null ? null : Barcode.Create(request.Barcode),
                VariantMapping.MapWeight(request.Weight, request.WeightUnit),
                dimensions,
                request.Images,
                ct: ct);

            await outboxStore.EnqueueAsync(
                new VariantUpdatedIntegrationEvent(
                    variation.ProductId,
                    variation.Id,
                    variation.Product.Name,
                    variation.Name,
                    variation.Sku.Value,
                    variation.Price.Value,
                    variation.Status.ToString(),
                    correlationId),
                ct);
        }, ct: ct);

        return new UpdateVariationResponse();
    }
}
