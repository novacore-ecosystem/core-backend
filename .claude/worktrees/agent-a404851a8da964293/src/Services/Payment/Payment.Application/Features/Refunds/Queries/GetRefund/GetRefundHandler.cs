using NovaCore.Payment.Application.Abstractions.Persistence.Refunds;

namespace NovaCore.Payment.Application.Features.Refunds.Queries.GetRefund;

public sealed class GetRefundHandler(IRefundReadService refundReadService) : IQueryHandler<GetRefundQuery, GetRefundResponse>
{
    public async Task<GetRefundResponse> Handle(GetRefundQuery request, CancellationToken ct = default)
    {
        var refund = await refundReadService.GetByIdAsync(request.RefundId, ct)
            ?? throw new NotFoundException(nameof(Refund), request.RefundId);

        return new GetRefundResponse(
            refund.Id,
            refund.PaymentId,
            refund.ReferenceType,
            refund.ReferenceId,
            refund.Amount.Amount,
            refund.Amount.Currency.Value,
            refund.Reason,
            refund.Status,
            refund.CreatedAt);
    }
}
