using NovaCore.BuildingBlock.Application.Abstractions.Common;

using Mapster;

using NovaCore.Promotion.Application.Abstractions.Persistence.Promotions;

namespace NovaCore.Promotion.Application.Features.Promotions.Queries.ListPromotions;

public sealed class ListPromotionsHandler(IPromotionReadService promotionReadService)
    : IQueryHandler<ListPromotionsQuery, PaginatedResult<PromotionSummaryResponse>>
{
    public async Task<PaginatedResult<PromotionSummaryResponse>> Handle(ListPromotionsQuery request, CancellationToken ct = default)
    {
        var (items, totalCount) = await promotionReadService.SearchAsync(request.Status, request.Page, request.PageSize, ct);

        var mapped = items.Adapt<List<PromotionSummaryResponse>>();

        return PaginatedResult<PromotionSummaryResponse>.Create(mapped, request.Page, request.PageSize, totalCount);
    }
}
