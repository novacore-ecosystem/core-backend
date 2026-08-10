using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

namespace NovaCore.Notification.Persistence.Engine.UnitOfWork;

/// <summary>
/// Mongo writes commit immediately per call (InsertOneAsync, ReplaceOneAsync, ...) - there is
/// no change tracker to flush the way EfUnitOfWork.SaveChangesAsync flushes one. This adapter
/// exists only so Application handlers can depend on the same IUnitOfWork abstraction every
/// other service uses; SaveChangesAsync is a documented no-op, not an oversight.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0);

    public async Task<bool> ExecuteTransactionAsync(
        Func<Task> action,
        Func<Task>? rollbackAction = null,
        CancellationToken ct = default)
    {
        try
        {
            await action();
            return true;
        }
        catch
        {
            if (rollbackAction is not null)
                await rollbackAction();

            throw;
        }
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
