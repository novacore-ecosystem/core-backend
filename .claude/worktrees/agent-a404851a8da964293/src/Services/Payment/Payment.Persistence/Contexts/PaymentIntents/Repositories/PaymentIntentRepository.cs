using NovaCore.Payment.Persistence.Engine;

namespace NovaCore.Payment.Persistence.Contexts.PaymentIntents.Repositories;

public sealed class PaymentIntentRepo(PaymentDbContext dbContext)
    : PaymentBaseRepository<PaymentIntent, Guid>(dbContext), IPaymentIntentRepository
{
}
