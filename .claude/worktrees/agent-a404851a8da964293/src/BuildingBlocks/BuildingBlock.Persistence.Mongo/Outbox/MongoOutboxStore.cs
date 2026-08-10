using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Persistence.Mongo.MongoContext;
using NovaCore.BuildingBlock.Persistence.Outbox;

using MongoDB.Driver;

namespace NovaCore.BuildingBlock.Persistence.Mongo.Outbox;

public sealed class MongoOutboxStore<TContext>(TContext context, ICurrentUserService currentUser) : IOutboxStore
    where TContext : MongoContextBase, IOutboxMongoContext
{
    private readonly TContext _context = context;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task EnqueueAsync(string eventType, string topic, string payload, string correlationId, CancellationToken ct = default)
    {
        var actorId = _currentUser.IsAuthenticated() ? _currentUser.GetUserId()?.ToString() : null;
        var actorType = actorId is not null ? "user" : "system";

        var message = OutboxDocument.Create(eventType, topic, payload, correlationId, actorId, actorType);
        await _context.OutboxMessages.InsertOneAsync(message, cancellationToken: ct);
    }

    public async Task<IReadOnlyList<OutboxMessageSnapshot>> GetUnprocessedAsync(int batchSize, CancellationToken ct = default)
    {
        var messages = await _context.OutboxMessages
            .Find(m => m.ProcessedAt == null)
            .SortBy(m => m.CreatedAt)
            .Limit(batchSize)
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
        var message = await _context.OutboxMessages.Find(m => m.Id == id).FirstOrDefaultAsync(ct);
        if (message is null)
            return;

        message.MarkProcessed();
        await _context.OutboxMessages.ReplaceOneAsync(m => m.Id == id, message, cancellationToken: ct);
    }

    public async Task MarkFailedAsync(Guid id, string error, CancellationToken ct = default)
    {
        var message = await _context.OutboxMessages.Find(m => m.Id == id).FirstOrDefaultAsync(ct);
        if (message is null)
            return;

        message.MarkFailed(error);
        await _context.OutboxMessages.ReplaceOneAsync(m => m.Id == id, message, cancellationToken: ct);
    }

    public async Task<int> DeleteProcessedBeforeAsync(DateTime olderThanUtc, int batchSize, CancellationToken ct = default)
    {
        var ids = await _context.OutboxMessages
            .Find(m => m.ProcessedAt != null && m.ProcessedAt < olderThanUtc)
            .SortBy(m => m.ProcessedAt)
            .Limit(batchSize)
            .Project(m => m.Id)
            .ToListAsync(ct);

        if (ids.Count == 0)
            return 0;

        var result = await _context.OutboxMessages.DeleteManyAsync(
            Builders<OutboxDocument>.Filter.In(m => m.Id, ids), ct);

        return (int)result.DeletedCount;
    }
}
