using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Persistence.Ef.UnitOfWork;

namespace NovaCore.Auth.Persistence.Engine.UnitOfWork;

public sealed class AuthUnitOfWork(AuthDbContext context)
    : EfUnitOfWork<AuthDbContext>(context), IUnitOfWork
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
