using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Persistence.Outbox;

using Microsoft.EntityFrameworkCore;

namespace NovaCore.BuildingBlock.Persistence.Ef.Outbox;

public sealed class EfOutboxStore<TContext>(TContext context, ICurrentUserService currentUser) : IOutboxStore
    where TContext : Microsoft.EntityFrameworkCore.DbContext, IOutboxDbContext
{
    private readonly TContext _context = context;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task EnqueueAsync(
        string eventType,
        string topic,
        string payload,
        string correlationId,
        CancellationToken ct = default)
    {
        var actorId = _currentUser.IsAuthenticated()
            ? _currentUser.GetUserId()?.ToString()
            : null;
        var actorType = actorId is not null ? "user" : "system";

        var message = OutboxMessage.Create(
            eventType,
            topic,
            payload,
            correlationId,
            actorId,
            actorType);
        await _context.OutboxMessages.AddAsync(message, ct);
    }

    public async Task<IReadOnlyList<OutboxMessageSnapshot>> GetUnprocessedAsync(
        int batchSize,
        CancellationToken ct = default)
    {
        var messages = await _context.OutboxMessages
            .AsNoTracking()
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);

        return [.. messages.Select(m => new OutboxMessageSnapshot(
            m.Id,
            m.EventType,
            m.Topic,
            m.Payload,
            m.CorrelationId,
            m.ActorId,
            m.ActorType,
            m.CreatedAt,
            m.ProcessedAt,
            m.Error,
            m.RetryCount))];
    }

    public async Task MarkProcessedAsync(Guid id, CancellationToken ct = default)
    {
        var message = await _context.OutboxMessages.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (message is null)
            return;

        message.MarkProcessed();
        await _context.SaveChangesAsync(ct);
    }

    public async Task MarkFailedAsync(Guid id, string error, CancellationToken ct = default)
    {
        var message = await _context.OutboxMessages.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (message is null)
            return;

        message.MarkFailed(error);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> DeleteProcessedBeforeAsync(
        DateTime olderThanUtc,
        int batchSize,
        CancellationToken ct = default)
    {
        var ids = await _context.OutboxMessages
            .Where(m => m.ProcessedAt != null && m.ProcessedAt < olderThanUtc)
            .OrderBy(m => m.ProcessedAt)
            .Take(batchSize)
            .Select(m => m.Id)
            .ToListAsync(ct);

        if (ids.Count == 0)
            return 0;

        await _context.OutboxMessages
            .Where(m => ids.Contains(m.Id))
            .ExecuteDeleteAsync(ct);

        return ids.Count;
    }
}
