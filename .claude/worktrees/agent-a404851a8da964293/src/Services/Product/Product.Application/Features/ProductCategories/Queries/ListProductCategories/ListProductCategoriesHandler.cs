using Mapster;

using NovaCore.Product.Application.Abstractions.Persistence.ProductCategories;

namespace NovaCore.Product.Application.Features.ProductCategories.Queries.ListProductCategories;

public sealed class ListProductCategoriesHandler(IProductCategoryReadService categoryReadService)
    : IQueryHandler<ListProductCategoriesQuery, ListProductCategoriesResponse>
{
    public async Task<ListProductCategoriesResponse> Handle(ListProductCategoriesQuery request, CancellationToken ct = default)
    {
        var categories = await categoryReadService.GetAllAsync(ct);

        return new ListProductCategoriesResponse(categories.Adapt<List<ProductCategoryItemResponse>>());
    }
}
