using NovaCore.Payment.Application.Abstractions.Persistence.Payments;
using NovaCore.Payment.Persistence.Contexts.Payments.Repositories;

namespace NovaCore.Payment.Persistence.Contexts.Payments.Read;

public sealed class PaymentReadService(IPaymentRepository paymentRepo) : IPaymentReadService
{
    public async Task<PaymentEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await paymentRepo.GetByIdAsync(
            id,
            query => query.Include(p => p.Items).Include(p => p.Attempts),
            ct);
    }

    public async Task<PaymentEntity?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default)
        => await paymentRepo.GetByIdempotencyKeyAsync(idempotencyKey, ct);
}
