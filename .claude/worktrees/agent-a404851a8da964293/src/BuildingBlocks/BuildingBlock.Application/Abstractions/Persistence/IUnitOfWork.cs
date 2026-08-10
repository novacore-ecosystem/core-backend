namespace NovaCore.BuildingBlock.Application.Abstractions.Persistence;

public interface IUnitOfWork : IAsyncDisposable
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task<bool> ExecuteTransactionAsync(
        Func<Task> action,
        Func<Task>? rollbackAction = null,
        CancellationToken ct = default);
}
