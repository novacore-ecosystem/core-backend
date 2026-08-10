using NovaCore.Payment.Application.Abstractions.Persistence.PaymentIntents;
using NovaCore.Payment.Persistence.Contexts.PaymentIntents.Repositories;

namespace NovaCore.Payment.Persistence.Contexts.PaymentIntents.Write;

public sealed class PaymentIntentWriteService(IPaymentIntentRepository repo) : IPaymentIntentWriteService
{
    public async Task<PaymentIntent> CreateAsync(
        ReferenceType referenceType,
        Guid referenceId,
        Money requestedAmount,
        DateTime? expiresAt,
        string? metadata,
        CancellationToken ct = default)
    {
        var intent = PaymentIntent.Create(referenceType, referenceId, requestedAmount, expiresAt, metadata);

        await repo.AddAsync(intent, ct);

        return intent;
    }
}
