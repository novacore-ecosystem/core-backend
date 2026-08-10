using NovaCore.BuildingBlock.Application.Exceptions;

using Mapster;

using NovaCore.Product.Application.Abstractions.Persistence.ProductCategories;

namespace NovaCore.Product.Application.Features.ProductCategories.Queries.GetProductCategory;

public sealed class GetProductCategoryHandler(IProductCategoryReadService categoryReadService)
    : IQueryHandler<GetProductCategoryQuery, GetProductCategoryResponse>
{
    public async Task<GetProductCategoryResponse> Handle(GetProductCategoryQuery request, CancellationToken ct = default)
    {
        var category = await categoryReadService.GetByIdAsync(request.ProductCategoryId, ct)
            ?? throw new NotFoundException(nameof(ProductCategory), request.ProductCategoryId);

        // ChildCategoryIds comes from a separate repository call, not the entity itself - map the
        // entity fields, then splice in the composed field via `with` (record non-destructive mutation).
        var childIds = await categoryReadService.GetChildIdsAsync(request.ProductCategoryId, ct);

        return category.Adapt<GetProductCategoryResponse>() with { ChildCategoryIds = childIds };
    }
}
