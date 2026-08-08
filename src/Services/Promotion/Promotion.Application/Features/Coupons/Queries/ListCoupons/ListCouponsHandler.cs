using NovaCore.BuildingBlock.Application.Abstractions.Common;

using Mapster;

using NovaCore.Promotion.Application.Abstractions.Persistence.Coupons;

namespace NovaCore.Promotion.Application.Features.Coupons.Queries.ListCoupons;

public sealed class ListCouponsHandler(ICouponReadService couponReadService)
    : IQueryHandler<ListCouponsQuery, PaginatedResult<CouponSummaryResponse>>
{
    public async Task<PaginatedResult<CouponSummaryResponse>> Handle(ListCouponsQuery request, CancellationToken ct = default)
    {
        var (items, totalCount) = await couponReadService.SearchAsync(request.Status, request.Page, request.PageSize, ct);

        var mapped = items.Adapt<List<CouponSummaryResponse>>();

        return PaginatedResult<CouponSummaryResponse>.Create(mapped, request.Page, request.PageSize, totalCount);
    }
}
