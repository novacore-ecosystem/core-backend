namespace NovaCore.Payment.Application.Abstractions.Persistence.Refunds;

public interface IRefundWriteService
{
    Task<Refund> CreateAsync(
        Guid paymentId,
        Money amount,
        string reason,
        CancellationToken ct = default);
}
