using NovaCore.Payment.Application.Abstractions.Persistence.Refunds;
using NovaCore.Payment.Persistence.Contexts.Refunds.Repositories;

namespace NovaCore.Payment.Persistence.Contexts.Refunds.Read;

public sealed class RefundReadService(IRefundRepository repo) : IRefundReadService
{
    public async Task<Refund?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await repo.GetByIdAsync(id, query => query.Include(r => r.Attempts), ct);
}
