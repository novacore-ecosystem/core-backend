using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Contract.Events.Product;

using NovaCore.Product.Application.Abstractions.Persistence.ProductCategories;
using NovaCore.Product.Application.Abstractions.Persistence.Products;

namespace NovaCore.Product.Application.Features.Products.Commands.AssignProductCategory;

public sealed class AssignProductCategoryHandler(
    IProductWriteService productWriteService,
    IProductCategoryReadService categoryReadService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<AssignProductCategoryCommand, AssignProductCategoryResponse>
{
    public async Task<AssignProductCategoryResponse> Handle(AssignProductCategoryCommand request, CancellationToken ct = default)
    {
        _ = await categoryReadService.GetByIdAsync(request.CategoryId, ct)
            ?? throw new NotFoundException(nameof(ProductCategory), request.CategoryId);

        var correlationId = currentUser.GetCorrelationId();

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await productWriteService.AssignCategoryAsync(
                request.ProductId,
                request.CategoryId,
                ct);

            await outboxStore.EnqueueAsync(
                new ProductCategoryAssignedIntegrationEvent(
                    request.ProductId,
                    request.CategoryId,
                    correlationId),
                ct);
        }, ct: ct);

        return new AssignProductCategoryResponse();
    }
}
