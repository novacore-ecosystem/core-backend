using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Contract.Events.Product;

using NovaCore.Product.Application.Abstractions.Persistence.Products;

namespace NovaCore.Product.Application.Features.Products.Commands.DeleteProduct;

/// <summary>Hard delete - matches this codebase's established convention (IRepository.DeleteAsync removes the row, no soft-delete flag exists on BaseEntity).</summary>
public sealed class DeleteProductHandler(
    IProductReadService productReadService,
    IProductWriteService productWriteService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<DeleteProductCommand, DeleteProductResponse>
{
    public async Task<DeleteProductResponse> Handle(DeleteProductCommand request, CancellationToken ct = default)
    {
        _ = await productReadService.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException(nameof(ProductEntity), request.ProductId);

        var correlationId = currentUser.GetCorrelationId();

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await productWriteService.DeleteAsync(request.ProductId, ct);

            await outboxStore.EnqueueAsync(
                new ProductDeletedIntegrationEvent(request.ProductId, correlationId),
                ct);
        }, ct: ct);

        return new DeleteProductResponse();
    }
}
