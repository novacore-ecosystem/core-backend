using NovaCore.Payment.Persistence.Engine;

namespace NovaCore.Payment.Persistence.Contexts.Payments.Repositories;

public sealed class PaymentRepo(PaymentDbContext dbContext)
    : PaymentBaseRepository<PaymentEntity, Guid>(dbContext), IPaymentRepository
{
    public async Task<PaymentEntity?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default)
    {
        return await _dbContext.Payments
            .AsNoTracking()
            .Include(p => p.Items)
            .Include(p => p.Attempts)
            .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey, ct);
    }
}
