using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Payment.Persistence.Contexts.Payments.Repositories;

public interface IPaymentRepository : IRepository<PaymentEntity, Guid>
{
    Task<PaymentEntity?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default);
}
