namespace NovaCore.Payment.Application.Features.Refunds.Queries.GetRefund;

public sealed record GetRefundQuery(Guid RefundId) : IQuery<GetRefundResponse>;

public sealed record GetRefundResponse(
    Guid Id,
    Guid PaymentId,
    ReferenceType ReferenceType,
    Guid ReferenceId,
    decimal Amount,
    string Currency,
    string Reason,
    RefundStatus Status,
    DateTime CreatedAt);
