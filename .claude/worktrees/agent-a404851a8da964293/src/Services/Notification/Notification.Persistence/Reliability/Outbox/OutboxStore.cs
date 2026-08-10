using System.Text.Json;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Contract.Events;

namespace NovaCore.Notification.Persistence.Reliability.Outbox;

public sealed class OutboxStore(
    NovaCore.BuildingBlock.Persistence.Outbox.IOutboxStore primitiveStore) : NovaCore.BuildingBlock.Application.Abstractions.Outbox.IOutboxStore
{
    private readonly NovaCore.BuildingBlock.Persistence.Outbox.IOutboxStore _primitiveStore = primitiveStore;

    public async Task EnqueueAsync<TEvent>(TEvent integrationEvent, CancellationToken ct = default)
        where TEvent : class, IIntegrationEvent
    {
        var topic = integrationEvent.EventType.ToLowerInvariant();
        var payload = JsonSerializer.Serialize(integrationEvent);

        await _primitiveStore.EnqueueAsync(
            integrationEvent.EventType,
            topic,
            payload,
            integrationEvent.CorrelationId,
            ct);
    }

    public async Task<IReadOnlyList<OutboxMessageSnapshot>> GetUnprocessedAsync(int batchSize, CancellationToken ct = default)
    {
        var primitiveRows = await _primitiveStore.GetUnprocessedAsync(batchSize, ct);
        return [.. primitiveRows.Select(row => new OutboxMessageSnapshot(
            row.Id,
            row.EventType,
            row.Topic,
            row.Payload,
            row.CorrelationId,
            row.ActorId,
            row.ActorType,
            row.CreatedAt,
            row.ProcessedAt,
            row.Error,
            row.RetryCount))];
    }

    public async Task MarkProcessedAsync(Guid id, CancellationToken ct = default)
    {
        await _primitiveStore.MarkProcessedAsync(id, ct);
    }

    public async Task MarkFailedAsync(Guid id, string error, CancellationToken ct = default)
    {
        await _primitiveStore.MarkFailedAsync(id, error, ct);
    }

    public async Task<int> DeleteProcessedBeforeAsync(DateTime olderThanUtc, int batchSize, CancellationToken ct = default)
    {
        return await _primitiveStore.DeleteProcessedBeforeAsync(olderThanUtc, batchSize, ct);
    }
}
