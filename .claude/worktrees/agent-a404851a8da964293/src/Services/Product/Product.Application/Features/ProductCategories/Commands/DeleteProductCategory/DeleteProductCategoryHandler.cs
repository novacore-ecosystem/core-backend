using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Product.Application.Abstractions.Persistence.ProductCategories;
using NovaCore.Product.Application.Abstractions.Persistence.Products;

namespace NovaCore.Product.Application.Features.ProductCategories.Commands.DeleteProductCategory;

public sealed class DeleteProductCategoryHandler(
    IProductCategoryReadService categoryReadService,
    IProductCategoryWriteService categoryWriteService,
    IProductReadService productReadService) : ICommandHandler<DeleteProductCategoryCommand, DeleteProductCategoryResponse>
{
    public async Task<DeleteProductCategoryResponse> Handle(DeleteProductCategoryCommand request, CancellationToken ct = default)
    {
        _ = await categoryReadService.GetByIdAsync(request.ProductCategoryId, ct)
            ?? throw new NotFoundException(nameof(ProductCategory), request.ProductCategoryId);

        if (await categoryReadService.HasChildrenAsync(request.ProductCategoryId, ct))
            throw new ConflictException("Cannot delete a category that has child categories.");

        if (await productReadService.ExistsWithCategoryAsync(request.ProductCategoryId, ct))
            throw new ConflictException("Cannot delete a category that is still assigned to products.");

        await categoryWriteService.DeleteAsync(request.ProductCategoryId, ct);

        return new DeleteProductCategoryResponse();
    }
}
