using NovaCore.BuildingBlock.Application.Abstractions.Common;

namespace NovaCore.Promotion.Application.Features.Coupons.Queries.ListCoupons;

public sealed record ListCouponsQuery(
    CouponStatus? Status,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedResult<CouponSummaryResponse>>;

public sealed record CouponSummaryResponse(
    Guid Id,
    string Code,
    string Name,
    CouponStatus Status,
    CouponVisibility Visibility,
    DateTime StartTime,
    DateTime EndTime,
    bool IsEnabled);
