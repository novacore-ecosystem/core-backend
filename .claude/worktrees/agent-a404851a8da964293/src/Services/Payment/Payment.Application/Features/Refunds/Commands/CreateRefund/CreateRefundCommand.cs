namespace NovaCore.Payment.Application.Features.Refunds.Commands.CreateRefund;

public sealed record CreateRefundCommand(
    Guid PaymentId,
    decimal Amount,
    string CurrencyCode,
    string Reason) : ICommand<CreateRefundResponse>;

public sealed record CreateRefundResponse(Guid RefundId, RefundStatus Status);
