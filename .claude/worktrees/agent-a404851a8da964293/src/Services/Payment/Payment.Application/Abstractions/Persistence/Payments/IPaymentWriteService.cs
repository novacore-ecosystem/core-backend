namespace NovaCore.Payment.Application.Abstractions.Persistence.Payments;

public interface IPaymentWriteService
{
    Task<PaymentEntity> CreateAsync(
        ReferenceType referenceType,
        Guid referenceId,
        Money amount,
        Guid gatewayId,
        Guid? paymentIntentId,
        Guid? paymentMethodId,
        string? idempotencyKey,
        DateTime? expiresAt,
        string? metadata,
        CancellationToken ct = default);
}
