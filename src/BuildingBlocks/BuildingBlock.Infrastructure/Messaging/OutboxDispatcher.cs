using System.Diagnostics;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Infrastructure.Observability;
using NovaCore.BuildingBlock.Messaging.Abstractions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NovaCore.BuildingBlock.Infrastructure.Messaging;

/// <summary>
/// Publishes a single Outbox row to Kafka and updates its Outbox state accordingly.
/// </summary>
/// <remarks>
/// Shared by both Outbox publish paths - OutboxRelayHostedService's background poll and the
/// immediate post-commit trigger wired into EfUnitOfWork - so a row is published with the same
/// message-id and interpreted (published/failed/retry-exhausted) identically regardless of which
/// path reaches it first. That shared message-id is what lets consumer-side Inbox dedup absorb a
/// row being published by both paths (e.g. immediate publish succeeds but the process crashes
/// before the row is marked processed, so the relay republishes it later).
/// </remarks>
public sealed class OutboxDispatcher(
    IOutboxStore outboxStore,
    IOutboxPublisher outboxPublisher,
    IOptions<OutboxRelayOptions> options,
    ILogger<OutboxDispatcher> logger)
{
    private readonly IOutboxStore _outboxStore = outboxStore;
    private readonly IOutboxPublisher _outboxPublisher = outboxPublisher;
    private readonly OutboxRelayOptions _options = options.Value;
    private readonly ILogger<OutboxDispatcher> _logger = logger;

    /// <summary>
    /// Publishes <paramref name="message"/> to Kafka and marks it processed on success, or
    /// failed (incrementing its retry count) on failure.
    /// </summary>
    /// <remarks>
    /// Never throws: a publish failure is logged and left for OutboxRelayHostedService to retry
    /// on its next poll, since Kafka publishing must never be allowed to affect a caller's
    /// already-committed business transaction.
    /// </remarks>
    /// <param name="message">The Outbox row to publish.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once the row has been published and marked, or marked failed.</returns>
    public async Task PublishAndMarkAsync(OutboxMessageSnapshot message, CancellationToken ct)
    {
        using var activity = InfrastructureActivitySource.Instance.StartActivity(
            "Outbox.PublishMessage", ActivityKind.Internal);
        activity?.SetTag("messaging.message.id", message.Id);
        activity?.SetTag("messaging.destination.name", message.Topic);

        try
        {
            await _outboxPublisher.PublishOutboxMessageAsync(
                message.Id,
                message.Topic,
                message.Payload,
                message.EventType,
                message.CorrelationId,
                message.ActorId,
                message.ActorType,
                ct);

            await _outboxStore.MarkProcessedAsync(message.Id, ct);
            _logger.LogInformation("Outbox message {MessageId} published successfully", message.Id);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _logger.LogError(ex, "Error publishing outbox message {MessageId}", message.Id);

            if (message.RetryCount < _options.MaxRetries)
            {
                await _outboxStore.MarkFailedAsync(message.Id, ex.Message, ct);
            }
            else
            {
                _logger.LogError("Outbox message {MessageId} exceeded max retries", message.Id);
                await _outboxStore.MarkFailedAsync(message.Id, $"Max retries exceeded: {ex.Message}", ct);
            }
        }
    }
}
