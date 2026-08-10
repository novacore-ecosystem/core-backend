namespace NovaCore.Payment.Application.Features.Payments.Commands.CreatePayment;

public sealed record CreatePaymentCommand(
    ReferenceType ReferenceType,
    Guid ReferenceId,
    decimal Amount,
    string CurrencyCode,
    Guid GatewayId,
    Guid? PaymentIntentId = null,
    Guid? PaymentMethodId = null,
    string? IdempotencyKey = null,
    DateTime? ExpiresAt = null,
    string? Metadata = null) : ICommand<CreatePaymentResponse>;

public sealed record CreatePaymentResponse(Guid PaymentId, PaymentStatus Status);
