namespace NovaCore.Payment.Application.Abstractions.Persistence.PaymentIntents;

public interface IPaymentIntentReadService
{
    Task<PaymentIntent?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
