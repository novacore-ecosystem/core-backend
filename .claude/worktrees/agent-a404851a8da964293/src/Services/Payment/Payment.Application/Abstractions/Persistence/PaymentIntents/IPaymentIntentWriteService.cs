namespace NovaCore.Payment.Application.Abstractions.Persistence.PaymentIntents;

public interface IPaymentIntentWriteService
{
    Task<PaymentIntent> CreateAsync(
        ReferenceType referenceType,
        Guid referenceId,
        Money requestedAmount,
        DateTime? expiresAt,
        string? metadata,
        CancellationToken ct = default);
}
