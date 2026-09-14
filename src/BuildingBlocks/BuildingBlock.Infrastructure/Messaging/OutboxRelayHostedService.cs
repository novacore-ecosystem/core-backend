using System.Diagnostics;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Infrastructure.Observability;

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
            var dispatcher = scope.ServiceProvider.GetRequiredService<OutboxDispatcher>();

            var messages = await outboxStore.GetUnprocessedAsync(_options.BatchSize, ct);
            activity?.SetTag("outbox.messages.count", messages.Count);

            if (messages.Count == 0)
                return;

            _logger.LogDebug("Processing {MessageCount} outbox messages", messages.Count);

            foreach (var message in messages)
            {
                await dispatcher.PublishAndMarkAsync(message, ct);
            }
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _logger.LogError(ex, "Error processing outbox messages");
        }
    }
}
