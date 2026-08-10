namespace NovaCore.Payment.Application.Abstractions.Persistence.Refunds;

public interface IRefundReadService
{
    Task<Refund?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
