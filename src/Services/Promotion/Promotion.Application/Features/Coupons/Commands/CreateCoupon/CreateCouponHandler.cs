using NovaCore.Promotion.Application.Abstractions.Persistence.Coupons;

namespace NovaCore.Promotion.Application.Features.Coupons.Commands.CreateCoupon;

/// <summary>
/// Phase 4.1 CQRS skeleton only - demonstrates the Command -&gt; Write Persistence Service -&gt;
/// UnitOfWork dependency shape a real Coupon-creation feature will follow (see
/// CreateProductCategoryHandler for the shape once ICouponWriteService gains a real Create
/// method: read-side uniqueness check, Coupon.Create(...), couponWriteService.CreateAsync(...)
/// inside uow.ExecuteTransactionAsync). No persistence method exists on ICouponReadService/
/// ICouponWriteService yet - adding one here would be exactly the speculative method Phase 4.1's
/// own brief forbids.
/// </summary>
public sealed class CreateCouponHandler(
    ICouponReadService couponReadService,
    ICouponWriteService couponWriteService,
    IUnitOfWork uow) : ICommandHandler<CreateCouponCommand, CreateCouponResponse>
{
    public Task<CreateCouponResponse> Handle(CreateCouponCommand request, CancellationToken ct = default)
    {
        // TODO: Implement real Coupon creation once the feature's actual contract is defined -
        // this will validate via couponReadService, construct the Coupon aggregate, and persist
        // it via couponWriteService inside a uow.ExecuteTransactionAsync block.
        throw new NotImplementedException("Coupon creation is not implemented yet - this is a Phase 4.1 CQRS skeleton only.");
    }
}
