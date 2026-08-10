using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Product.Application.Abstractions.Persistence.ProductCategories;

namespace NovaCore.Product.Application.Features.ProductCategories.Commands.UpdateProductCategory;

public sealed class UpdateProductCategoryHandler(
    IProductCategoryReadService categoryReadService,
    IProductCategoryWriteService categoryWriteService,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateProductCategoryCommand, UpdateProductCategoryResponse>
{
    public async Task<UpdateProductCategoryResponse> Handle(UpdateProductCategoryCommand request, CancellationToken ct = default)
    {
        if (request.ParentCategoryId is not null)
        {
            _ = await categoryReadService.GetByIdAsync(request.ParentCategoryId.Value, ct)
                ?? throw new NotFoundException(nameof(ProductCategory), request.ParentCategoryId.Value);

            await EnsureNoCycleAsync(request.ProductCategoryId, request.ParentCategoryId.Value, ct);
        }

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await categoryWriteService.UpdateDetailsAsync(
                request.ProductCategoryId, request.Name.Trim(), request.Description.Trim(), request.ParentCategoryId, ct);
        }, ct: ct);

        return new UpdateProductCategoryResponse();
    }

    // A category can only guard against direct self-parenting on its own (see
    // ProductCategory.ChangeParent) - detecting "moving under one of my own descendants"
    // requires walking the tree across other aggregate instances, which is this Application
    // handler's job, not the Domain's.
    private async Task EnsureNoCycleAsync(Guid categoryId, Guid proposedParentId, CancellationToken ct)
    {
        var currentId = (Guid?)proposedParentId;
        var guard = 0;

        while (currentId is not null && guard++ < 100)
        {
            if (currentId == categoryId)
                throw new ConflictException("Cannot move a category under one of its own descendants.");

            var current = await categoryReadService.GetByIdAsync(currentId.Value, ct);
            currentId = current?.ParentCategoryId;
        }
    }
}
