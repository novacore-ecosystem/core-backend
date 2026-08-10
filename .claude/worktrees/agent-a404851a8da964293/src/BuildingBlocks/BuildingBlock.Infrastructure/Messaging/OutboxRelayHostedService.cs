using System.Diagnostics;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Infrastructure.Observability;
using NovaCore.BuildingBlock.Messaging.Abstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NovaCore.BuildingBlock.Infrastructure.Messaging;

/// <summary>
/// Polls the Outbox for unprocessed messages and publishes them to Kafka.
/// Runs continuously on a configurable interval, processing messages in batches.
/// Handles transient failures with retries and logs persistent errors.
/// Messages are published with message-id headers for Inbox deduplication.
/// </summary>
public sealed class OutboxRelayHostedService(
    IServiceProvider serviceProvider,
    IOptions<OutboxRelayOptions> options,
    ILogger<OutboxRelayHostedService> logger)
    : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly OutboxRelayOptions _options = options.Value;
    private readonly ILogger<OutboxRelayHostedService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox relay hosted service starting");

        using var timer = new PeriodicTimer(_options.PollingInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Outbox relay hosted service stopping");
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken ct)
    {
        // No parent HTTP request exists here, so this is a root span - it's what makes the
        // relay's periodic poll show up as its own named transaction in APM instead of an
        // unnamed/DB-named one.
        using var activity = InfrastructureActivitySource.Instance.StartActivity(
            "OutboxRelay.Poll", ActivityKind.Internal);

        try
        {
            using var scope = _serviceProvider.CreateAsyncScope();
            var outboxStore = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
            var outboxPublisher = scope.ServiceProvider.GetRequiredService<IOutboxPublisher>();

            var messages = await outboxStore.GetUnprocessedAsync(_options.BatchSize, ct);
            activity?.SetTag("outbox.messages.count", messages.Count);

            if (messages.Count == 0)
                return;

            _logger.LogDebug("Processing {MessageCount} outbox messages", messages.Count);

            foreach (var message in messages)
            {
                await PublishAndMarkAsync(message, outboxStore, outboxPublisher, ct);
            }
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _logger.LogError(ex, "Error processing outbox messages");
        }
    }

    private async Task PublishAndMarkAsync(
        OutboxMessageSnapshot message,
        IOutboxStore outboxStore,
        IOutboxPublisher outboxPublisher,
        CancellationToken ct)
    {
        using var activity = InfrastructureActivitySource.Instance.StartActivity(
            "OutboxRelay.PublishMessage", ActivityKind.Internal);
        activity?.SetTag("messaging.message.id", message.Id);
        activity?.SetTag("messaging.destination.name", message.Topic);

        try
        {
            await outboxPublisher.PublishOutboxMessageAsync(
                message.Id,
                message.Topic,
                message.Payload,
                message.EventType,
                message.CorrelationId,
                message.ActorId,
                message.ActorType,
                ct);

            await outboxStore.MarkProcessedAsync(message.Id, ct);
            _logger.LogInformation("Outbox message {MessageId} published successfully", message.Id);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _logger.LogError(ex, "Error publishing outbox message {MessageId}", message.Id);

            if (message.RetryCount < _options.MaxRetries)
            {
                await outboxStore.MarkFailedAsync(message.Id, ex.Message, ct);
            }
            else
            {
                _logger.LogError("Outbox message {MessageId} exceeded max retries", message.Id);
                await outboxStore.MarkFailedAsync(message.Id, $"Max retries exceeded: {ex.Message}", ct);
            }
        }
    }
}
