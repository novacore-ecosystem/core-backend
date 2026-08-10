using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Contract.Events.Product;

using NovaCore.Product.Application.Abstractions.Persistence.Products;

namespace NovaCore.Product.Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductHandler(
    IProductReadService productReadService,
    IProductWriteService productWriteService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<UpdateProductCommand, UpdateProductResponse>
{
    public async Task<UpdateProductResponse> Handle(UpdateProductCommand request, CancellationToken ct = default)
    {
        if (await productReadService.SlugExistsAsync(request.Slug, request.ProductId, ct))
            throw new ConflictException($"Product with slug ({request.Slug}) already exists");

        var slug = Slug.Create(request.Slug);
        var correlationId = currentUser.GetCorrelationId();

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await productWriteService.UpdateDetailsAsync(
                request.ProductId,
                request.Name,
                request.Description,
                slug,
                ct);

            await outboxStore.EnqueueAsync(
                new ProductUpdatedIntegrationEvent(
                    request.ProductId,
                    request.Name,
                    slug.Value,
                    correlationId),
                ct);
        }, ct: ct);

        return new UpdateProductResponse();
    }
}
