using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Npgsql;

namespace NovaCore.BuildingBlock.Persistence.Ef.UnitOfWork;

/// <summary>
/// EF Core unit of work: runs a business action inside a single transaction, then saves and
/// commits it as one atomic step.
/// </summary>
/// <param name="context">The EF Core context this unit of work commits.</param>
/// <param name="notifyOutboxCommittedAsync">
/// Optional post-commit publish trigger. Invoked once per successful <see cref="ExecuteTransactionAsync"/>
/// call that created new Outbox rows, with the ids of the rows that just committed - never before
/// commit, and never after a rollback. Wired by NovaCore.BuildingBlock.Infrastructure.Messaging so
/// services that enqueue an Outbox row inside their unit of work get an immediate Kafka publish
/// attempt automatically, without any per-handler code. Left null for services that have not wired
/// Kafka messaging, in which case rows are only ever picked up by the Outbox relay's background poll.
/// </param>
public abstract class EfUnitOfWork<TDbContext>(
    TDbContext context,
    Func<IReadOnlyList<Guid>, CancellationToken, Task>? notifyOutboxCommittedAsync = null) : IAsyncDisposable
    where TDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    protected readonly TDbContext Context = context;
    private readonly Func<IReadOnlyList<Guid>, CancellationToken, Task>? _notifyOutboxCommittedAsync = notifyOutboxCommittedAsync;
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

                // Captured before SaveChanges: at this point the rows are still tracked as
                // Added, so this is exactly the set of Outbox rows this transaction is about to
                // commit - no need for `action` or its caller to report them separately.
                var committedOutboxIds = Context.ChangeTracker.Entries<OutboxMessage>()
                    .Where(e => e.State == EntityState.Added)
                    .Select(e => e.Entity.Id)
                    .ToArray();

                await Context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                await NotifyOutboxCommittedAsync(committedOutboxIds, ct);

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

    /// <summary>
    /// Best-effort immediate-publish trigger, fired once per commit that created new Outbox
    /// rows.
    /// </summary>
    /// <remarks>
    /// Never allowed to affect the result of the transaction it follows: the transaction has
    /// already committed by the time this runs, so a publish failure here must not surface as a
    /// failure of <see cref="ExecuteTransactionAsync"/>. The delegate implementation (see
    /// NovaCore.BuildingBlock.Infrastructure.Messaging.OutboxDispatcher) owns its own
    /// publish/mark-processed/mark-failed error handling and is not expected to throw; the
    /// try/catch here is a last-resort guard on top of that. Either way, a failure just leaves
    /// the row for the Outbox relay's next poll to pick up, exactly as if this trigger did not
    /// exist.
    /// </remarks>
    /// <param name="committedOutboxIds">Ids of the Outbox rows the just-committed transaction created.</param>
    /// <param name="ct">Cancellation token.</param>
    private async Task NotifyOutboxCommittedAsync(IReadOnlyList<Guid> committedOutboxIds, CancellationToken ct)
    {
        if (_notifyOutboxCommittedAsync is null || committedOutboxIds.Count == 0)
            return;

        try
        {
            await _notifyOutboxCommittedAsync(committedOutboxIds, ct);
        }
        catch
        {
            // Swallowed by design - see remarks above.
        }
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
