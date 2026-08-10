using NovaCore.BuildingBlock.Application.Exceptions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Npgsql;

namespace NovaCore.BuildingBlock.Persistence.Ef.UnitOfWork;

public abstract class EfUnitOfWork<TDbContext>(TDbContext context) : IAsyncDisposable
    where TDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    protected readonly TDbContext Context = context;
    private IDbContextTransaction? _transaction;

    public virtual Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return Context.SaveChangesAsync(ct);
    }

    public virtual async Task<bool> ExecuteTransactionAsync(
        Func<Task> action,
        Func<Task>? rollbackAction = null,
        CancellationToken ct = default)
    {
        var strategy = Context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await Context.Database.BeginTransactionAsync(ct);

            _transaction = transaction;

            try
            {
                await action();

                await Context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return true;
            }
            catch (Exception ex)
            {
                if (rollbackAction is not null)
                {
                    await rollbackAction();
                }

                await transaction.RollbackAsync(ct);

                // A manual RollbackAsync does not revert the change tracker: entities added or
                // modified inside `action` (or staged by a caller before invoking this method,
                // e.g. an optimistic Inbox completion marker) remain tracked as Added/Modified
                // even though none of it is actually in the database anymore. Clear() detaches
                // everything so a caller inspecting entity state afterward sees "nothing
                // committed", and so the context stays safe to reuse for a subsequent attempt.
                Context.ChangeTracker.Clear();

                // Every existing caller awaits this method without checking the returned bool,
                // so a swallowed failure here previously meant the caller silently proceeded as
                // if the transaction had succeeded. Rethrowing makes callers actually observe
                // failures - required for retry logic (Inbox, etc.) to detect them at all.
                //
                // DbUpdateConcurrencyException is translated to the Application-layer
                // ConflictException here (rather than left as a raw EF type) so callers above
                // Persistence - which must stay EF-agnostic per Clean Architecture - can catch a
                // known type to drive their own retry loop (see Inventory's DeductStockHandler).
                if (ex is DbUpdateConcurrencyException)
                    throw new ConflictException("The record was modified concurrently. Please retry.");

                // A unique-index violation (Postgres 23505) means two concurrent requests raced
                // past an application-level existence pre-check (e.g. AddVariation's SkuExistsAsync)
                // and both tried to insert the same value. Without this, the raw PostgresException
                // bubbles up unhandled and surfaces as a 500 instead of the 409 the pre-check would
                // have produced had it lost the race the other way.
                if (ex is DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } })
                    throw new ConflictException("The record already exists.");

                throw;
            }
            finally
            {
                _transaction = null;
            }
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
        }

        await Context.DisposeAsync();
    }
}
