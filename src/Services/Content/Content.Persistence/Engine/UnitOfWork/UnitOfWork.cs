using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Persistence.Ef.UnitOfWork;

using NovaCore.Content.Persistence.Engine;

namespace NovaCore.Content.Persistence.Engine.UnitOfWork;

public sealed class UnitOfWork(
    ContentDbContext context,
    Func<IReadOnlyList<Guid>, CancellationToken, Task>? notifyOutboxCommittedAsync = null)
    : EfUnitOfWork<ContentDbContext>(context, notifyOutboxCommittedAsync), IUnitOfWork
{
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await base.SaveChangesAsync(ct);
    }

    public override async Task<bool> ExecuteTransactionAsync(
        Func<Task> action,
        Func<Task>? rollbackAction = null,
        CancellationToken ct = default)
    {
        return await base.ExecuteTransactionAsync(
            action,
            rollbackAction,
            ct);
    }
}
