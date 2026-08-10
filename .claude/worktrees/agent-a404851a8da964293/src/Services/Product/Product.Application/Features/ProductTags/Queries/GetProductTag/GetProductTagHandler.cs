using NovaCore.BuildingBlock.Application.Exceptions;

using Mapster;

using NovaCore.Product.Application.Abstractions.Persistence.ProductTags;

namespace NovaCore.Product.Application.Features.ProductTags.Queries.GetProductTag;

public sealed class GetProductTagHandler(IProductTagReadService tagReadService)
    : IQueryHandler<GetProductTagQuery, GetProductTagResponse>
{
    public async Task<GetProductTagResponse> Handle(GetProductTagQuery request, CancellationToken ct = default)
    {
        var tag = await tagReadService.GetByIdAsync(request.ProductTagId, ct)
            ?? throw new NotFoundException(nameof(ProductTag), request.ProductTagId);

        return tag.Adapt<GetProductTagResponse>();
    }
}
