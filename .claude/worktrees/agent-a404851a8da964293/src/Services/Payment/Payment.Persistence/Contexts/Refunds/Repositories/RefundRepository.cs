using NovaCore.Payment.Persistence.Engine;

namespace NovaCore.Payment.Persistence.Contexts.Refunds.Repositories;

public sealed class RefundRepo(PaymentDbContext dbContext)
    : PaymentBaseRepository<Refund, Guid>(dbContext), IRefundRepository
{
}
