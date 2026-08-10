using NovaCore.Payment.Application.Abstractions.Persistence.PaymentIntents;
using NovaCore.Payment.Persistence.Contexts.PaymentIntents.Repositories;

namespace NovaCore.Payment.Persistence.Contexts.PaymentIntents.Read;

public sealed class PaymentIntentReadService(IPaymentIntentRepository repo) : IPaymentIntentReadService
{
    public async Task<PaymentIntent?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await repo.GetByIdAsync(id, ct);
}
