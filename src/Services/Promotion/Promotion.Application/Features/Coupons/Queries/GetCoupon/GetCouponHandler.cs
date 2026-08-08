using NovaCore.Promotion.Application.Abstractions.Persistence.Coupons;

namespace NovaCore.Promotion.Application.Features.Coupons.Queries.GetCoupon;

/// <summary>
/// Phase 4.1 CQRS skeleton only - demonstrates the Query -&gt; Read Persistence Service dependency
/// shape a real Coupon-lookup feature will follow (see GetProductCategoryHandler for the shape
/// once ICouponReadService gains a real GetByIdAsync-shaped method: fetch the Coupon, throw
/// NotFoundException when missing, map to GetCouponResponse via Mapster's .Adapt&lt;T&gt;() - the
/// project's established Entity -&gt; Response pattern, not demonstrated live here since it
/// requires exactly the persistence method Phase 4.1's own brief forbids adding).
/// </summary>
public sealed class GetCouponHandler(ICouponReadService couponReadService) : IQueryHandler<GetCouponQuery, GetCouponResponse>
{
    public Task<GetCouponResponse> Handle(GetCouponQuery request, CancellationToken ct = default)
    {
        // TODO: Implement once ICouponReadService gains a real lookup method - fetch via
        // couponReadService, throw NotFoundException when missing, map via .Adapt<GetCouponResponse>().
        throw new NotImplementedException("Coupon retrieval is not implemented yet - this is a Phase 4.1 CQRS skeleton only.");
    }
}
