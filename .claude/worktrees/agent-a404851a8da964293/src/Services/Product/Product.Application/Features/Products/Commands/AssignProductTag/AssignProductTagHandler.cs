using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Contract.Events.Product;

using NovaCore.Product.Application.Abstractions.Persistence.Products;
using NovaCore.Product.Application.Abstractions.Persistence.ProductTags;

namespace NovaCore.Product.Application.Features.Products.Commands.AssignProductTag;

public sealed class AssignProductTagHandler(
    IProductWriteService productWriteService,
    IProductTagReadService tagReadService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<AssignProductTagCommand, AssignProductTagResponse>
{
    public async Task<AssignProductTagResponse> Handle(AssignProductTagCommand request, CancellationToken ct = default)
    {
        _ = await tagReadService.GetByIdAsync(request.TagId, ct)
            ?? throw new NotFoundException(nameof(ProductTag), request.TagId);

        var correlationId = currentUser.GetCorrelationId();

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await productWriteService.AssignTagAsync(
                request.ProductId,
                request.TagId,
                ct);

            await outboxStore.EnqueueAsync(
                new ProductTagAssignedIntegrationEvent(
                    request.ProductId,
                    request.TagId,
                    correlationId),
                ct);
        }, ct: ct);

        return new AssignProductTagResponse();
    }
}
