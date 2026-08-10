namespace NovaCore.Payment.Application.Features.PaymentIntents.Queries.GetPaymentIntent;

public sealed record GetPaymentIntentQuery(Guid PaymentIntentId) : IQuery<GetPaymentIntentResponse>;

public sealed record GetPaymentIntentResponse(
    Guid Id,
    ReferenceType ReferenceType,
    Guid ReferenceId,
    decimal RequestedAmount,
    string Currency,
    PaymentIntentStatus Status,
    string ClientSecret,
    DateTime? ExpiresAt,
    DateTime CreatedAt);
