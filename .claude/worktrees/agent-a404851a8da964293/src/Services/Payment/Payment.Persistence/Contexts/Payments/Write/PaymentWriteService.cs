using NovaCore.Payment.Application.Abstractions.Persistence.Payments;
using NovaCore.Payment.Persistence.Contexts.Payments.Repositories;

namespace NovaCore.Payment.Persistence.Contexts.Payments.Write;

public sealed class PaymentWriteService(IPaymentRepository paymentRepo) : IPaymentWriteService
{
    public async Task<PaymentEntity> CreateAsync(
        ReferenceType referenceType,
        Guid referenceId,
        Money amount,
        Guid gatewayId,
        Guid? paymentIntentId,
        Guid? paymentMethodId,
        string? idempotencyKey,
        DateTime? expiresAt,
        string? metadata,
        CancellationToken ct = default)
    {
        var payment = PaymentEntity.Create(
            referenceType,
            referenceId,
            amount,
            gatewayId,
            paymentIntentId,
            paymentMethodId,
            idempotencyKey,
            expiresAt,
            metadata);

        await paymentRepo.AddAsync(payment, ct);

        return payment;
    }
}
