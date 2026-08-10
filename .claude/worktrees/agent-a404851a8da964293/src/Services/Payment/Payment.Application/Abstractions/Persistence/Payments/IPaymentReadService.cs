namespace NovaCore.Payment.Application.Abstractions.Persistence.Payments;

public interface IPaymentReadService
{
    Task<PaymentEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<PaymentEntity?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default);
}
