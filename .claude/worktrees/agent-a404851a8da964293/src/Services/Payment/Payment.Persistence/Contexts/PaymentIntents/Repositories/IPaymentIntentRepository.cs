using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Payment.Persistence.Contexts.PaymentIntents.Repositories;

public interface IPaymentIntentRepository : IRepository<PaymentIntent, Guid>
{
}
