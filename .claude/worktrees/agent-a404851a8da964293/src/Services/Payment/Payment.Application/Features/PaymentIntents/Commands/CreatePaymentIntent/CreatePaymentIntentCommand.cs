namespace NovaCore.Payment.Application.Features.PaymentIntents.Commands.CreatePaymentIntent;

public sealed record CreatePaymentIntentCommand(
    ReferenceType ReferenceType,
    Guid ReferenceId,
    decimal RequestedAmount,
    string CurrencyCode,
    DateTime? ExpiresAt = null,
    string? Metadata = null) : ICommand<CreatePaymentIntentResponse>;

public sealed record CreatePaymentIntentResponse(Guid PaymentIntentId, string ClientSecret, PaymentIntentStatus Status);
